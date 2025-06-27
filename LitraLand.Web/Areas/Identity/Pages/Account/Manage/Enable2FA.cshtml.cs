using Hangfire;
using LitraLand.Domain.Entities.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LitraLand.Web.Areas.Identity.Pages.Account.Manage
{
    public class Enable2FAModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;

        public Enable2FAModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IEmailBodyBuilder emailBodyBuilder)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _emailBodyBuilder = emailBodyBuilder;
        }

        // This method is triggered when the user visits the page (GET request)
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("Unable to load user.");

            var is2FAEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (is2FAEnabled)
                return RedirectToPage("./Index"); // Redirect to the index page if 2FA is already enabled

            return Page();
        }

        // This method is triggered when the user submits the form (POST request)
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Unable to load user.");
            }

            // generate a 2FA code
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

            var placeholders = new Dictionary<string, string>()
            {
                { "header", $"Hello {user.UserName}," },
                { "body", "To secure your account, we need you to enter the verification code below." },
                { "code", code }
            };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.TwoFactorAuthentication, placeholders);

            // Send the email with the verification code
            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
                user.Email!,
                "Your 2FA Verification Code",
                body
            ));

            // After sending the code, redirect to a page where the user can input the code
            TempData["Allow2FAVerification"] = true; // this is to check if the user is coming from the Enable2FA page or trying to access the Verify2FACode page directly (in the OnGetAsync method of Verify2FACode page we'll check this)
            return RedirectToPage("./Verify2FACode"); // We'll create this page later
        }
    }
}