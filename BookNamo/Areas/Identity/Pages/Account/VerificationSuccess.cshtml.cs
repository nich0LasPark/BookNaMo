// PageModel for the email verification success page. Displays a confirmation message after successful OTP verification.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookNamo.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class VerificationSuccessModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? Email { get; set; } // Make nullable

        public void OnGet(string? email) // Add nullable parameter
        {
            Email = email;
        }
    }
}
