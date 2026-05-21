using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InvoiceProcessingWebApp.Pages;

[Authorize]
[ValidateAntiForgeryToken]
public class SettingsModel : PageModel
{
    [TempData]
    public string? SuccessMessage { get; set; }

    [BindProperty]
    public string AppName { get; set; } = "Invoxa";

    [BindProperty]
    public string DateFormat { get; set; } = "MMM dd, yyyy";

    [BindProperty]
    public string Currency { get; set; } = "USD";

    [BindProperty]
    public int PageSize { get; set; } = 10;

    [BindProperty]
    public bool EmailNotifications { get; set; } = true;

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();
        SuccessMessage = "Settings saved successfully!";
        return RedirectToPage();
    }
}
