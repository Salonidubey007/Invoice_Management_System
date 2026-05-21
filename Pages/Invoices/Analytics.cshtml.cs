using Microsoft.AspNetCore.Mvc.RazorPages;
using InvoiceProcessingWebApp.Data;

namespace InvoiceProcessingWebApp.Pages.Invoices;

public class AnalyticsModel : PageModel
{
    private readonly AppDbContext _context;

    public AnalyticsModel(AppDbContext context)
    {
        _context = context;
    }

    public int Total { get; set; }
    public int NewCount { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public string TopVendor { get; set; } = "N/A";
    public List<MonthlyData> MonthlyTotals { get; set; } = new();

    public void OnGet(int page = 1, int pageSize = 10)
    {
        // Implement pagination for large data sets
        var invoices = _context.Invoices.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        Total = invoices.Count;
        NewCount = invoices.Count(i => i.Status == "New");
        PendingCount = invoices.Count(i => i.Status == "Pending");
        ApprovedCount = invoices.Count(i => i.Status == "Approved");
        RejectedCount = invoices.Count(i => i.Status == "Rejected");
        TotalAmount = invoices.Any() ? invoices.Sum(i => i.Amount) : 0;
        ApprovedAmount = invoices.Where(i => i.Status == "Approved").Any()
            ? invoices.Where(i => i.Status == "Approved").Sum(i => i.Amount) : 0;

        TopVendor = invoices.GroupBy(i => i.VendorName)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault() ?? "N/A";

        MonthlyTotals = invoices
            .GroupBy(i => new { i.InvoiceDate.Year, i.InvoiceDate.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyData
            {
                Label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                Count = g.Count(),
                Amount = g.Sum(i => i.Amount)
            }).ToList();
    }

    public class MonthlyData
    {
        public string Label { get; set; } = "";
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }
}
