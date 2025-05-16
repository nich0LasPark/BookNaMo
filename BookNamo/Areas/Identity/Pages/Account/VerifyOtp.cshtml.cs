// Handles OTP verification logic. Confirms user email if the correct OTP is entered.
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BookNamo.Models;
using BookNamo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BookNamo.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class VerifyOtpModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly OtpService _otpService;
        private readonly ILogger<VerifyOtpModel> _logger;

        public VerifyOtpModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            OtpService otpService,
            ILogger<VerifyOtpModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _otpService = otpService;
            _logger = logger;
        }

        [BindProperty]
        [Required(ErrorMessage = "Verification code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "The verification code must be 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "The verification code must contain 6 digits only")]
        [Display(Name = "Verification Code")]
        public string? OtpCode { get; set; } // Add nullable annotation

        [BindProperty]
        [Required]
        [EmailAddress]
        public string? Email { get; set; } // Add nullable annotation

        [TempData]
        public string? StatusMessage { get; set; } // Add nullable annotation

        [TempData]
        public bool EmailVerified { get; set; }

        public IActionResult OnGet(string? email) // Add nullable annotation
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Index");
            }

            Email = email;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null) // Add nullable annotation
        {
            // Use null coalescing operator with non-null value
            returnUrl = returnUrl ?? Url.Content("~/"); // Fix for CS8625

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Email ?? "");
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid verification attempt.");
                return Page();
            }

            var isValid = await _otpService.ValidateOtpAsync(user, OtpCode ?? "");
            if (isValid)
            {
                // Confirm the email since OTP validated successfully
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User email confirmed through OTP for user {UserId}", user.Id);

                // Set success message
                EmailVerified = true;
                StatusMessage = "Email verification successful! Your account is now active.";

                // If we should sign in immediately
                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToPage("./VerificationSuccess", new { email = Email });
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid verification code or the code has expired.");
                return Page();
            }
        }

        public async Task<IActionResult> OnGetResendCodeAsync(string? email) // Add nullable annotation
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Index");
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Don't reveal that the user doesn't exist
                    return RedirectToPage("./VerifyOtp", new { email });
                }

                // Generate and save a new OTP
                var otp = _otpService.GenerateOtp();
                await _otpService.SaveOtpForUserAsync(user, otp);

                // Send it to the user's email
                await _otpService.SendOtpEmailAsync(user, otp);

                StatusMessage = "A new verification code has been sent to your email.";
                return RedirectToPage("./VerifyOtp", new { email });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Multiple accounts found with the same email address: {Email}", email);
                StatusMessage = "A system error occurred. Please contact support.";
                return RedirectToPage("./VerifyOtp", new { email });
            }
        }
    }
}
