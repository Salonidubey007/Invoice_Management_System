using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InvoiceProcessingWebApp.Data;
using InvoiceProcessingWebApp.Models;

namespace InvoiceProcessingWebApp.Pages.Invoices;

[Authorize]
[ValidateAntiForgeryToken]
public class EditModel : PageModel
{
    private readonly AppDbContext _context;

    public EditModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Invoice Invoice { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null) return NotFound();
        Invoice = invoice;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var existing = _context.Invoices.Find((int)Invoice.Id);
        if (existing == null) return NotFound();

        existing.InvoiceNumber = Invoice.InvoiceNumber;
        existing.VendorName = Invoice.VendorName;
        existing.Amount = Invoice.Amount;
        existing.InvoiceDate = Invoice.InvoiceDate;
        existing.PurchaseOrderNumber = Invoice.PurchaseOrderNumber;
        existing.Status = Invoice.Status;

        await _context.SaveChangesAsync();
        return RedirectToPage("/Invoices/Index");
    }
}
