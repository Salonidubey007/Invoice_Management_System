using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InvoiceProcessingWebApp.Data;

namespace InvoiceProcessingWebApp.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(AppDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public int TotalInvoices { get; set; }
    public int ApprovedCount { get; set; }
    public int PendingCount { get; set; }
    public int RejectedCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public string WelcomeName { get; set; } = "User";

    public async Task OnGetAsync()
    {
        TotalInvoices = _context.Invoices.Count();
        ApprovedCount = _context.Invoices.Count(i => i.Status == "Approved");
        PendingCount = _context.Invoices.Count(i => i.Status == "Pending");
        RejectedCount = _context.Invoices.Count(i => i.Status == "Rejected");
        TotalAmount = _context.Invoices.Any() ? _context.Invoices.Sum(i => i.Amount) : 0;
        ApprovedAmount = _context.Invoices.Any(i => i.Status == "Approved")
            ? _context.Invoices.Where(i => i.Status == "Approved").Sum(i => i.Amount) : 0;

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
                WelcomeName = user.Email ?? user.UserName ?? "User";
        }
    }
}
