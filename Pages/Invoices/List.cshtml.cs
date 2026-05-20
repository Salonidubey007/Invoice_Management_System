using Microsoft.AspNetCore.Mvc.RazorPages;
using InvoiceProcessingWebApp.Data;
using InvoiceProcessingWebApp.Models;

namespace InvoiceProcessingWebApp.Pages.Invoices;

public class ListModel : PageModel
{
    private readonly AppDbContext _context;

    public ListModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Invoice> Invoices { get; set; } = new();

    public void OnGet()
    {
        Invoices = _context.Invoices.ToList();
    }
}
