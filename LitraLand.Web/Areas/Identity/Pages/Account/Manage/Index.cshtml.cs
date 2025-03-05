// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LitraLand.Web.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IImageServices _imageServices;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IImageServices imageServices)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _imageServices = imageServices;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

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
        public class InputModel
        {
            [Required, MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Full Name")]
            [RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
            public string FullName { get; set; } = null!;

            [Phone]
            [Display(Name = "Phone number"), MaxLength(11, ErrorMessage = Errors.MaxLength)]
            [RegularExpression(RegexPatterns.MobileNumber, ErrorMessage = Errors.InvalidMobileNumber)]
            public string PhoneNumber { get; set; }

            public IFormFile Avatar { get; set; }

            public bool ImageRemoved { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                FullName = user.FullName,
                PhoneNumber = phoneNumber
            };
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

            if(Input.Avatar is not null)
            {
                var isAllowedExt = _imageServices.IsAllowedImageExtension(Input.Avatar, new[] {".png"});

                if (!isAllowedExt)
                {
                    ModelState.AddModelError("Input.Avatar", "Invalid Image format. Only .png is allowed.");
                    await LoadAsync(user);
                    return Page();
                }

                _imageServices.Delete($"/images/users/{user.Id}.png");

                var (isUploaded, errorMessage) = await _imageServices.UploadAsync(Input.Avatar, $"{user.Id}.png", "/images/users", hasThumbnail: false);

                if (!isUploaded)
                {
                    ModelState.AddModelError("Input.Avatar", errorMessage);
                    await LoadAsync(user);
                    return Page();
                }
            }
            else if (Input.ImageRemoved)
                _imageServices.Delete($"/images/users/{user.Id}.png");

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if(Input.FullName != user.FullName)
            {
                user.FullName = Input.FullName;
                var result = await _userManager.UpdateAsync(user);
                if(!result.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set FullName.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
