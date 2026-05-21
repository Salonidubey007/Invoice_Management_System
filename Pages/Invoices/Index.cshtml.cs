using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InvoiceProcessingWebApp.Data;
using InvoiceProcessingWebApp.Models;
using System.Text;

namespace InvoiceProcessingWebApp.Pages.Invoices;

[Authorize]
[ValidateAntiForgeryToken]
public class IndexModel : PageModel
{
    private readonly AppDbContext _context;
    private const int PageSize = 10;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Invoice> Invoices { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }

    public void OnGet(int pageNumber = 1)
    {
        CurrentPage = pageNumber < 1 ? 1 : pageNumber;
        TotalCount = _context.Invoices.Count();
        TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
        if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

        // Implement pagination for large data sets
        var invoices = _context.Invoices.OrderByDescending(i => i.InvoiceDate).Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
        Invoices = invoices;
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        // Corrected to use Entity Framework's FindAsync method
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice != null)
        {
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public IActionResult OnGetExport()
    {
        var invoices = _context.Invoices.OrderByDescending(i => i.InvoiceDate).ToList();
        var sb = new StringBuilder();
        sb.AppendLine("Invoice Number,Vendor Name,Amount,Date,PO Number,Status");
        foreach (var inv in invoices)
        {
            // Escape fields to prevent CSV injection
            sb.AppendLine($"\"{inv.InvoiceNumber}\",\"{inv.VendorName}\",{inv.Amount},{inv.InvoiceDate:yyyy-MM-dd},\"{inv.PurchaseOrderNumber}\",\"{inv.Status}\"");
        }
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "invoices.csv");
    }
}
