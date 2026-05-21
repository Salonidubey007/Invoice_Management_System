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

    public new int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public void OnGet()
    {
        Invoices = _context.Invoices.Skip((Page - 1) * PageSize).Take(PageSize).ToList();
    }
}
