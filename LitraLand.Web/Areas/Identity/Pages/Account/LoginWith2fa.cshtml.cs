// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Hangfire;
using LitraLand.Domain.Entities.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LitraLand.Web.Areas.Identity.Pages.Account
{
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginWith2faModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;

        public LoginWith2faModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginWith2faModel> logger,
            IEmailBodyBuilder emailBodyBuilder,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
        }

        public string MaskedEmail { get; set; } = string.Empty;

        [BindProperty]
        public InputModel Input { get; set; }

        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

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

            [Display(Name = "Remember this machine")]
            public bool RememberMachine { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;
            MaskedEmail = MaskEmail(user.Email!);

            // Check if the TempData contains the "ReturnUrl" key and set it if it does (to avoid losing the return URL if the user refreshes the page)
            if (!string.IsNullOrEmpty(ReturnUrl))
                TempData["ReturnUrl"] = ReturnUrl;

            // this workaround is to check if the code was sent before or not
            // (if the code was sent, we don't need to send it again in the OnGetAsync method, the user can use the OnGetResendAsync method to resend the code)
            if (TempData.Peek("CodeSent") == null)
            {
                await GenerateAndSendTwoFactorCodeAsync(user);
                TempData["CodeSent"] = true;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //returnUrl ??= Url.Content("~/");
            returnUrl ??= ReturnUrl ?? Url.Content("~/");

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var verificationCode = GetFullCode();

            if (string.IsNullOrWhiteSpace(verificationCode) || verificationCode.Length != 6 || !verificationCode.All(char.IsDigit))
            {
                ModelState.AddModelError(string.Empty, "Please enter a valid 6-digit code.");
                return Page();
            }

            var result = await _signInManager.TwoFactorSignInAsync("Email", verificationCode, rememberMe, Input.RememberMachine);

            if (result.Succeeded)
            {
                TempData.Remove("CodeSent"); // 
                _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToPage("./Lockout");
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return Page();
            }
        }


        public async Task<IActionResult> OnGetResendAsync()
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
                throw new InvalidOperationException("Unable to load two-factor authentication user.");

            // Generate and send a new 2FA code
            await GenerateAndSendTwoFactorCodeAsync(user);
            TempData["CodeSent"] = true;
            TempData["SuccessMessage"] = "A new verification code has been sent to your email address.";

            return RedirectToPage(new { rememberMe = RememberMe, returnUrl = ReturnUrl });
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

        private async Task GenerateAndSendTwoFactorCodeAsync(ApplicationUser user)
        {
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            if (string.IsNullOrEmpty(code))
                throw new InvalidOperationException($"Unable to generate two-factor authentication code.");

            var placeholders = new Dictionary<string, string>()
            {
                { "header", $"Hello {user.UserName}," },
                { "body", "please enter the verification code below in the login prompt to complete the login process." },
                { "code", code }
            };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.TwoFactorAuthentication, placeholders);

            // Send the email with the verification code
            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
                user.Email!,
                "Your 2FA Verification Code",
                body
            ));
        }

    }
}