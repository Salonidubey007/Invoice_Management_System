using Microsoft.AspNetCore.Mvc.RazorPages;
using InvoiceProcessingWebApp.Data;

namespace InvoiceProcessingWebApp.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public int TotalInvoices { get; set; }
    public int ApprovedCount { get; set; }
    public int PendingCount { get; set; }
    public decimal TotalAmount { get; set; }

    public void OnGet()
    {
        TotalInvoices = _context.Invoices.Count();
        ApprovedCount = _context.Invoices.Count(i => i.Status == "Approved");
        PendingCount = _context.Invoices.Count(i => i.Status == "Pending");
        TotalAmount = _context.Invoices.Any() ? _context.Invoices.Sum(i => i.Amount) : 0;
    }
}
