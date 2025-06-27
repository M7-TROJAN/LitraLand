using LitraLand.Domain.Entities.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LitraLand.Web.Areas.Identity.Pages.Account.Manage
{
    public class Verify2FACodeModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public Verify2FACodeModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public string MaskedEmail { get; set; } = string.Empty;

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        [BindProperty]
        public InputModel Input { get; set; }


        public class InputModel
        {

            [Required, MaxLength(1)]
            [RegularExpression(RegexPatterns.NumbersOnly, ErrorMessage = "Only numbers are allowed.")]
            public string Code1 { get; set; } = string.Empty;

            [Required, MaxLength(1)]
            [RegularExpression(RegexPatterns.NumbersOnly, ErrorMessage = "Only numbers are allowed.")]
            public string Code2 { get; set; } = string.Empty;

            [Required, MaxLength(1)]
            [RegularExpression(RegexPatterns.NumbersOnly, ErrorMessage = "Only numbers are allowed.")]
            public string Code3 { get; set; } = string.Empty;

            [Required, MaxLength(1)]
            [RegularExpression(RegexPatterns.NumbersOnly, ErrorMessage = "Only numbers are allowed.")]
            public string Code4 { get; set; } = string.Empty;

            [Required, MaxLength(1)]
            [RegularExpression(RegexPatterns.NumbersOnly, ErrorMessage = "Only numbers are allowed.")]
            public string Code5 { get; set; } = string.Empty;

            [Required, MaxLength(1)]
            [RegularExpression(RegexPatterns.NumbersOnly, ErrorMessage = "Only numbers are allowed.")]
            public string Code6 { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // check if the user is coming from the Enable2FA page or he is trying to access this page directly
            if (TempData["Allow2FAVerification"] == null)
                return RedirectToPage("./Enable2FA"); // رجّعه للصفحة اللي بدأ منها

            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return NotFound("User not found.");

            // check if 2FA is enabled
            var is2FAEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (is2FAEnabled)
                return RedirectToPage("./Enable2FA");

            MaskedEmail = MaskEmail(user.Email!);
            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("User not found.");

            var fullCode = GetFullCode();

            if (string.IsNullOrWhiteSpace(fullCode) || fullCode.Length != 6 || !fullCode.All(char.IsDigit))
            {
                ModelState.AddModelError(string.Empty, "Please enter a valid 6-digit code.");
                StatusMessage = "Error Invalid verification code.";
                return Page();
            }

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", fullCode);
            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid verification code.");
                StatusMessage = "Error Invalid verification code.";
                return Page();
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);

            TempData["SuccessMessage"] = "Two-Factor Authentication has been enabled.";
            StatusMessage = "Success Two-Factor Authentication has been enabled.";
            return RedirectToPage("./Index");
        }


        private string MaskEmail(string email)
        {
            var parts = email.Split('@');
            if (parts.Length != 2 || parts[0].Length < 4)
                return email;

            string localPart = parts[0];
            string maskedLocal = new string('*', localPart.Length - 3) + localPart[^3..]; // آخر 3 حروف
            return maskedLocal + "@" + parts[1];
        }

        private string GetFullCode()
        {
            return $"{Input?.Code1}{Input?.Code2}{Input?.Code3}{Input?.Code4}{Input?.Code5}{Input?.Code6}";
        }
    }
}