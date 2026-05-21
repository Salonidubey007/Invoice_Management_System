using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace InvoiceProcessingWebApp.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;

    public ForgotPasswordModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public bool ShowResetForm { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email")]
        public string Email { get; set; } = "";

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }

        public string? Step { get; set; }
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.Step == "verify")
        {
            // Step 1: verify email exists
            if (string.IsNullOrEmpty(Input.Email))
            {
                ErrorMessage = "Please enter your email.";
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ErrorMessage = "No account found with that email address.";
                return Page();
            }

            ShowResetForm = true;
            return Page();
        }
        else
        {
            // Step 2: reset password
            if (string.IsNullOrEmpty(Input.NewPassword) || string.IsNullOrEmpty(Input.ConfirmPassword))
            {
                ErrorMessage = "Please enter and confirm your new password.";
                ShowResetForm = true;
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ErrorMessage = "User not found.";
                return Page();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, Input.NewPassword);

            if (result.Succeeded)
                return RedirectToPage("/Account/Login", new { message = "passwordreset" });

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            ShowResetForm = true;
            return Page();
        }
    }
}
