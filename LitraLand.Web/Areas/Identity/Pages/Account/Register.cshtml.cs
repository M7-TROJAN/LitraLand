// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Hangfire;
using LitraLand.Domain.Entities.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

namespace LitraLand.Web.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IEmailBodyBuilder emailBodyBuilder)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _emailBodyBuilder = emailBodyBuilder;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required]
            [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "First Name")]
            [RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
            public string FirstName { get; set; }

            [Required]
            [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Last Name")]
            [RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
            public string LastName { get; set; }

            [Required]
            [MaxLength(20, ErrorMessage = Errors.MaxLength), Display(Name = "Username")]
            [RegularExpression(RegexPatterns.Username, ErrorMessage = Errors.InvalidUsername)]
            public string Username { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 8)]
            [RegularExpression(RegexPatterns.Password, ErrorMessage = Errors.PasswordComplexity)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = Errors.ConfirmPasswordNotMatch)]
            public string ConfirmPassword { get; set; }

        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Validate email and username
                var hasValidationErrors = await ValidateUserUniquenessAsync(Input.Email, Input.Username);
                if (hasValidationErrors)
                {
                    return Page();
                }

                var user = CreateUser();

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);

                    // set the user to the role of CommunityMember
                    await _userManager.AddToRoleAsync(user, AppRoles.CommunityMember);

                    // start send email confirmation
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);


                    var placeholders = new Dictionary<string, string>()
                    {
                        { "userName" , user.UserName },
                        { "url", HtmlEncoder.Default.Encode(callbackUrl!) }
                    };

                    var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.EmailConfirmation, placeholders);

                    BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
                        Input.Email,
                        "Confirm your email",
                        body
                    ));

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        //return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                        return RedirectToPage("./RegisterConfirmation", new { email = Input.Email, fullName = user.FullName, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            var user = new ApplicationUser
            {
                UserName = Input.Username,
                Email = Input.Email,
                FullName = $"{Input.FirstName} {Input.LastName}",
                AppllicationArea = AppConstants.CommunityArea
            };

            return user;
        }

        private async Task<bool> ValidateUserUniquenessAsync(string email, string username)
        {
            var hasError = false;

            var existingEmailUser = await _userManager.FindByEmailAsync(email);
            if (existingEmailUser != null)
            {
                ModelState.AddModelError("Input.Email", "Email is already registered.");
                hasError = true;
            }

            var existingUsernameUser = await _userManager.FindByNameAsync(username);
            if (existingUsernameUser != null)
            {
                ModelState.AddModelError("Input.Username", "Username is already taken.");
                hasError = true;
            }

            return hasError;
        }

    }
}