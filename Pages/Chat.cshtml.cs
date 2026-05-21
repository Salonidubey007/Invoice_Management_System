using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InvoiceProcessingWebApp.Data;
using InvoiceProcessingWebApp.Services;
using Microsoft.EntityFrameworkCore;

namespace InvoiceProcessingWebApp.Pages
{
    [Authorize]
    public class ChatModel : PageModel
    {
        private readonly RagService _rag;
        private readonly AppDbContext _db;

        public ChatModel(RagService rag, AppDbContext db)
        {
            _rag = rag;
            _db = db;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsk([FromBody] QuestionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Question))
                return new JsonResult(new { answer = "Question is empty." });
            if (request.Question.Length > 500)
                return new JsonResult(new { answer = "Question is too long. Please keep it under 500 characters." });
            try
            {
                var answer = await _rag.AskAsync(request.Question);
                return new JsonResult(new { answer });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { answer = $"Error: {ex.Message}" });
            }
        }

        public async Task<IActionResult> OnPostIndexInvoices()
        {
            try
            {
                await _rag.IndexInvoicesAsync();
                var count = await _db.Invoices.CountAsync();
                return new JsonResult(new { message = $"{count} invoices indexed successfully." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { message = $"Indexing failed: {ex.Message}" });
            }
        }

        public async Task<IActionResult> OnPostIndexPdf()
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
                return new JsonResult(new { message = "No file uploaded." });
            if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) &&
                !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return new JsonResult(new { message = "Only PDF files are allowed." });
            if (file.Length > 10 * 1024 * 1024)
                return new JsonResult(new { message = "File size must be under 10MB." });
            try
            {
                using var stream = file.OpenReadStream();
                await _rag.IndexPdfAsync(file.FileName, stream);
                return new JsonResult(new { message = $"'{file.FileName}' indexed successfully." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { message = $"PDF indexing failed: {ex.Message}" });
            }
        }
    }

    public class QuestionRequest
    {
        public string? Question { get; set; }
    }
}
