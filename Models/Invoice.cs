using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceProcessingWebApp.Models;

public class Invoice
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string InvoiceNumber { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string VendorName { get; set; } = null!;

    [Range(0.01, 1000000)]
    public decimal Amount { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime InvoiceDate { get; set; }

    [StringLength(50)]
    public string PurchaseOrderNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "New";
}
