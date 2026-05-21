using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Pinecone;
using Pinecone.Rest;
using UglyToad.PdfPig;
using InvoiceProcessingWebApp.Data;

namespace InvoiceProcessingWebApp.Services;

public class RagService
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpFactory;
    private readonly AppDbContext _db;
    private readonly ILogger<RagService> _logger;

    private string GeminiApiKey => _config["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini:ApiKey is not configured.");
    private string EmbeddingModel => _config["Gemini:EmbeddingModel"] ?? "text-embedding-004";
    private string ChatModel => _config["Gemini:ChatModel"] ?? "gemini-1.5-flash";
    private string PineconeApiKey => _config["Pinecone:ApiKey"] ?? throw new InvalidOperationException("Pinecone:ApiKey is not configured.");
    private string PineconeHost => _config["Pinecone:Host"] ?? throw new InvalidOperationException("Pinecone:Host is not configured.");
    private string PineconeIndexName => _config["Pinecone:IndexName"] ?? "invoice";

    public RagService(IConfiguration config, IHttpClientFactory httpFactory, AppDbContext db, ILogger<RagService> logger)
    {
        _config = config;
        _httpFactory = httpFactory;
        _db = db;
        _logger = logger;
    }

    private async Task<Index<RestTransport>> GetIndexAsync()
    {
        var pinecone = new PineconeClient(PineconeApiKey, new Uri(PineconeHost));
        return await pinecone.GetIndex<RestTransport>(PineconeIndexName);
    }

    public List<string> ExtractChunks(Stream pdfStream, int chunkSize = 400)
    {
        var chunks = new List<string>();
        using var doc = PdfDocument.Open(pdfStream);
        var sb = new StringBuilder();
        foreach (var page in doc.GetPages())
            sb.Append(page.Text).Append(' ');

        var words = sb.ToString().Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i += chunkSize)
            chunks.Add(string.Join(' ', words.Skip(i).Take(chunkSize)));

        return chunks;
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var client = _httpFactory.CreateClient();
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{EmbeddingModel}:embedContent?key={GeminiApiKey}";
        var body = JsonSerializer.Serialize(new
        {
            model = $"models/{EmbeddingModel}",
            content = new { parts = new[] { new { text } } }
        });

        var response = await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini embedding error: {Status} {Body}", response.StatusCode, json);
            throw new Exception($"Gemini embedding failed: {response.StatusCode}");
        }

        using var parsed = JsonDocument.Parse(json);
        var values = parsed.RootElement
            .GetProperty("embedding").GetProperty("values")
            .EnumerateArray().Select(v => v.GetSingle()).ToArray();

        if (values.Length < 1024) values = values.Concat(new float[1024 - values.Length]).ToArray();
        else if (values.Length > 1024) values = values.Take(1024).ToArray();

        return values;
    }

    public async Task IndexPdfAsync(string fileName, Stream pdfStream)
    {
        var safeFileName = Regex.Replace(
            Path.GetFileName(fileName), @"[^\w.\-]", "_",
            RegexOptions.None, TimeSpan.FromSeconds(2));

        var chunks = ExtractChunks(pdfStream);
        if (chunks.Count == 0) return;

        var vectors = new List<Vector>();
        for (int i = 0; i < chunks.Count; i++)
        {
            var embedding = await GetEmbeddingAsync(chunks[i]);
            vectors.Add(new Vector
            {
                Id = $"{safeFileName}-chunk-{i}",
                Values = embedding,
                Metadata = new MetadataMap
                {
                    ["text"] = chunks[i],
                    ["source"] = safeFileName,
                    ["chunk"] = i
                }
            });
        }

        using var index = await GetIndexAsync();
        await index.Upsert(vectors);
        _logger.LogInformation("Indexed {Count} chunks from PDF {File}", vectors.Count, safeFileName);
    }

    public async Task IndexInvoicesAsync()
    {
        var totalCount = _db.Invoices.Count();
        if (totalCount == 0) return;

        using var index = await GetIndexAsync();
        const int batchSize = 100;

        for (int skip = 0; skip < totalCount; skip += batchSize)
        {
            var batch = _db.Invoices
                .OrderBy(i => i.Id)
                .Skip(skip)
                .Take(batchSize)
                .ToList();

            var vectors = new List<Vector>();
            foreach (var inv in batch)
            {
                var text = $"Invoice {inv.InvoiceNumber} from vendor {inv.VendorName}, amount {inv.Amount:C}, date {inv.InvoiceDate:MMM dd yyyy}, status {inv.Status}, PO {inv.PurchaseOrderNumber}.";
                var embedding = await GetEmbeddingAsync(text);
                vectors.Add(new Vector
                {
                    Id = $"invoice-db-{inv.Id}",
                    Values = embedding,
                    Metadata = new MetadataMap
                    {
                        ["text"] = text,
                        ["source"] = "database",
                        ["invoiceId"] = inv.Id
                    }
                });
            }
            await index.Upsert(vectors);
        }

        _logger.LogInformation("Indexed {Count} invoices from database", totalCount);
    }

    public async Task<List<string>> SearchAsync(string question, int topK = 5)
    {
        var embedding = await GetEmbeddingAsync(question);
        using var index = await GetIndexAsync();
        var results = await index.Query(embedding, (uint)topK, includeMetadata: true);

        return results
            .Where(m => m.Metadata != null && m.Metadata.ContainsKey("text"))
            .Select(m => m.Metadata!["text"].ToString() ?? "")
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();
    }

    public async Task<string> AskAsync(string question)
    {
        var contexts = await SearchAsync(question);
        var contextText = contexts.Any()
            ? string.Join("\n\n", contexts.Select((c, i) => $"[{i + 1}] {c}"))
            : "No relevant invoice context found.";

        var prompt = $"""
            You are Invoxa AI, an assistant strictly for an invoice management system.
            Only answer questions about invoices, vendors, amounts, statuses, dates, and purchase orders.
            If the question is unrelated, say: "I can only answer questions about invoices and related data."

            Context:
            {contextText}

            Question: {question}

            Answer concisely based only on the context above:
            """;

        var client = _httpFactory.CreateClient();
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{ChatModel}:generateContent?key={GeminiApiKey}";
        var body = JsonSerializer.Serialize(new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { temperature = 0.1, maxOutputTokens = 512 }
        });

        var response = await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini chat error: {Status} {Body}", response.StatusCode, json);
            return "Sorry, I couldn't get an answer right now. Please try again.";
        }

        using var parsed = JsonDocument.Parse(json);
        return parsed.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "No response.";
    }
}
