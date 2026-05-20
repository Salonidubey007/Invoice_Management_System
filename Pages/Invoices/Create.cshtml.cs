using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InvoiceProcessingWebApp.Data;
using InvoiceProcessingWebApp.Models;

namespace InvoiceProcessingWebApp.Pages.Invoices;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Invoice Invoice { get; set; } = new Invoice();

    public void OnGet()
    {
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.Invoices.Add(Invoice);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Invoices/Index");
    }
}
