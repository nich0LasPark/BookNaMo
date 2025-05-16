// PageModel for managing user profile information (e.g., updating username).

#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BookNamo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BookNamo.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        [Display(Name = "Username")]
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            Username = user.UserName ?? string.Empty;

        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var currentUsername = await _userManager.GetUserNameAsync(user);

            if (Username != currentUsername)
            {
                var setUsernameResult = await _userManager.SetUserNameAsync(user, Username);
                if (!setUsernameResult.Succeeded)
                {
                    var errors = string.Join(", ", setUsernameResult.Errors.Select(e => e.Description));
                    StatusMessage = $"Error: {errors}";
                    ModelState.AddModelError(string.Empty, errors);
                    await LoadAsync(user);
                    return Page();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your username has been updated";
            return RedirectToPage();
        }
    }
}
