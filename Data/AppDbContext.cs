using Microsoft.EntityFrameworkCore;
using InvoiceProcessingWebApp.Models;

namespace InvoiceProcessingWebApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Invoice> Invoices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>()
            .Property(invoice => invoice.Amount)
            .HasPrecision(18, 2);
    }
}
