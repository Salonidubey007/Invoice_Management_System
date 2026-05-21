# Invoxa — Smart Invoicing Simplified

A production-ready ASP.NET Core 10 web application for managing invoices, with an AI-powered assistant backed by Google Gemini and Pinecone vector search.

---

## Features

- **Invoice Management** — Create, edit, delete, and list invoices with pagination
- **PDF Upload** — Upload PDF invoices (max 10 MB, validated)
- **CSV Export** — Export all invoices to CSV
- **Analytics Dashboard** — Charts and stats across invoice statuses and amounts
- **AI Assistant** — Ask natural-language questions about your invoices using RAG (Retrieval-Augmented Generation)
- **Authentication** — ASP.NET Core Identity with lockout, secure cookies, and HTTPS
- **Security** — Global CSRF protection, security headers, input validation, XSS-safe rendering
- **Structured Logging** — Serilog with daily rolling file and console sinks

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 10 Razor Pages |
| ORM | Entity Framework Core 10 |
| Database (dev) | SQLite |
| Database (prod) | SQL Server |
| Auth | ASP.NET Core Identity |
| AI Chat | Google Gemini (`gemini-1.5-flash`) |
| Embeddings | Google Gemini (`text-embedding-004`) |
| Vector DB | Pinecone (REST, 1024 dimensions) |
| PDF Parsing | PdfPig |
| Logging | Serilog |

---

## Project Structure

```
InvoiceProcessingWebApp/
├── Data/
│   └── AppDbContext.cs          # EF Core DbContext with Identity
├── Models/
│   └── Invoice.cs               # Invoice entity
├── Pages/
│   ├── Account/
│   │   ├── Login.cshtml(.cs)    # Login page
│   │   ├── Logout.cshtml(.cs)   # Logout handler
│   │   └── Register.cshtml(.cs) # Registration page
│   ├── Invoices/
│   │   ├── Index.cshtml(.cs)    # Invoice list with pagination
│   │   ├── Create.cshtml(.cs)   # Create invoice
│   │   ├── Edit.cshtml(.cs)     # Edit invoice
│   │   ├── Upload.cshtml(.cs)   # PDF upload
│   │   └── Analytics.cshtml(.cs)# Charts and stats
│   ├── Shared/
│   │   └── _Layout.cshtml       # Shared layout with sidebar
│   ├── Chat.cshtml(.cs)         # AI assistant page
│   ├── Index.cshtml(.cs)        # Dashboard
│   └── Settings.cshtml(.cs)     # App settings
├── Services/
│   └── RagService.cs            # Gemini embeddings + Pinecone RAG
├── Uploads/                     # Uploaded PDF files (gitignored)
├── Logs/                        # Serilog daily log files (gitignored)
├── appsettings.json             # Config placeholders (no secrets)
└── Program.cs                   # App startup and middleware pipeline
```

---

## Screenshots

### Login
![Login](Ui_images/Login%20page.png)

### Register
![Register](Ui_images/Sign_up%20page.png)

### Dashboard
![Dashboard](Ui_images/Dashboard%20page.png)

### Invoice List
![Invoices](Ui_images/Invoices%20page.png)

### Create Invoice
![Create Invoice](Ui_images/Create%20Invoice%20page.png)

### Upload Invoice
![Upload](Ui_images/UploadInvoice%20page.png)

### Analytics
![Analytics](Ui_images/Analytics%20Page.png)

### AI Assistant
![AI Assistant](Ui_images/AI%20Assistant.png)

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A [Google Gemini API key](https://aistudio.google.com/app/apikey)
- A [Pinecone](https://www.pinecone.io/) account with an index named `invoice` (1024 dimensions, cosine metric)

### 1. Clone and restore

```bash
git clone <repo-url>
cd InvoiceProcessingWebApp
dotnet restore
```

### 2. Configure secrets (never commit real keys)

Use .NET user secrets for local development:

```bash
dotnet user-secrets set "Gemini:ApiKey" "<your-gemini-api-key>"
dotnet user-secrets set "Pinecone:ApiKey" "<your-pinecone-api-key>"
dotnet user-secrets set "Pinecone:Host" "<your-pinecone-host-url>"
```

For production, set these as environment variables or use a secrets manager.

### 3. Run

```bash
dotnet run
```

The app auto-migrates the SQLite database on first run. Navigate to `https://localhost:<port>` and register an account.

---

## AI Assistant Usage

The AI assistant answers questions strictly about your invoice data.

1. Go to **AI Assistant** in the sidebar
2. Click **Index All Invoices** — this embeds all DB invoices into Pinecone (run after adding/editing invoices)
3. Ask questions like:
   - *"How many invoices are pending?"*
   - *"What is the total approved amount?"*
   - *"Which vendor has the most invoices?"*

The assistant uses RAG: your question is embedded, matched against Pinecone vectors, and the top results are passed as context to Gemini for a grounded answer.

---

## Production Deployment

### Switch to SQL Server

Update `appsettings.json` (or environment variable):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=<host>;Database=InvoxaDb;User Id=<user>;Password=<pass>;"
}
```

The app detects `Server=` in the connection string and switches from SQLite to SQL Server automatically.

### Environment variables (recommended over appsettings)

```
Gemini__ApiKey=<key>
Pinecone__ApiKey=<key>
Pinecone__Host=<host>
ConnectionStrings__DefaultConnection=<connection-string>
```

### Security checklist before going live

- [ ] Real API keys are in environment variables, not `appsettings.json`
- [ ] `appsettings.json` is in `.gitignore`
- [ ] HTTPS is enforced (`UseHttpsRedirection` + `UseHsts` are active in non-dev)
- [ ] Database is SQL Server or PostgreSQL, not SQLite
- [ ] `Uploads/` folder is outside the web root or access-restricted
- [ ] Serilog log files are not publicly accessible

---

## Security

| Protection | Implementation |
|---|---|
| CSRF | `AutoValidateAntiforgeryTokenAttribute` applied globally in `Program.cs` |
| Authentication | All invoice and chat pages require `[Authorize]` |
| Account lockout | 5 failed attempts → 15-minute lockout |
| Security headers | `X-Content-Type-Options`, `X-Frame-Options`, `X-XSS-Protection`, `Referrer-Policy` |
| XSS | Chat messages rendered with `textContent`, never `innerHTML` |
| File upload | PDF-only, 10 MB max, sanitized filename, path traversal check |
| AI input | Max 500 characters per question |
| Log injection | Filenames sanitized before logging |

---

## License

MIT
