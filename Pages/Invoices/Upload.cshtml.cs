using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InvoiceProcessingWebApp.Pages.Invoices;

[ValidateAntiForgeryToken]
public class UploadModel : PageModel
{
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
    private static readonly string[] AllowedExtensions = { ".pdf" };

    public string? Message { get; set; }
    public bool IsSuccess { get; set; }

    public async Task<IActionResult> OnPostAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            Message = "No file selected.";
            return Page();
        }

        if (file.Length > MaxFileSize)
        {
            Message = "File size exceeds the 10 MB limit.";
            return Page();
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            Message = "Only PDF files are allowed.";
            return Page();
        }

        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        // Sanitize filename — strip directory components, use a safe unique name
        var safeFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.GetFullPath(Path.Combine(uploadsPath, safeFileName));

        // Ensure the resolved path is still inside the Uploads folder
        if (!fullPath.StartsWith(Path.GetFullPath(uploadsPath)))
        {
            Message = "Invalid file path.";
            return Page();
        }

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        IsSuccess = true;
        Message = $"File '{file.FileName}' uploaded successfully!";
        return Page();
    }
}
