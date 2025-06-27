// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using LitraLand.Domain.Entities.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LitraLand.Web.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IImageServices _imageServices;
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public IndexModel(
            IApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IMapper mapper,
            IImageServices imageServices)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _imageServices = imageServices;
        }

        public bool Is2FAEnabled { get; set; }

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
            [Required, MaxLength(20, ErrorMessage = Errors.MaxLength), Display(Name = "Username")]
            [RegularExpression(RegexPatterns.Username, ErrorMessage = Errors.InvalidUsername)]
            public string Username { get; set; }

            [Required, MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Full Name")]
            [RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
            public string FullName { get; set; } = null!;

            [Phone]
            [Display(Name = "Phone number"), MaxLength(11, ErrorMessage = Errors.MaxLength)]
            [RegularExpression(RegexPatterns.MobileNumber, ErrorMessage = Errors.InvalidMobileNumber)]
            public string PhoneNumber { get; set; }

            [Display(Name = "Date of Birth")]
            [AssertThat("DateOfBirth <= Today()", ErrorMessage = Errors.NotAllowFutureDates)]
            public DateTime DateOfBirth { get; set; } = DateTime.Now;

            [Display(Name = "Area")]
            [Required(ErrorMessage = Errors.Required)]
            public int? AreaId { get; set; }
            public IEnumerable<SelectListItem> Areas { get; set; } = new List<SelectListItem>();

            [Display(Name = "Governorate")]
            [Required(ErrorMessage = Errors.Required)]
            public int? GovernorateId { get; set; }
            public IEnumerable<SelectListItem> Governorates { get; set; }

            [MaxLength(500, ErrorMessage = Errors.MaxLength)]
            public string Address { get; set; } = null!;

            public string ImageUrl { get; set; } = null!;
            public string ImageThumbnailUrl { get; set; } = null!;

            public IFormFile Avatar { get; set; }

            public bool ImageRemoved { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Input = new InputModel
            {
                Username = userName,
                FullName = user.FullName,
                PhoneNumber = phoneNumber,
                DateOfBirth = user.DateOfBirth,
                ImageUrl = user.ImageUrl,
                ImageThumbnailUrl = user.ImageThumbnailUrl,
                AreaId = user.AreaId,
                GovernorateId = user.GovernorateId,
                Address = user.Address
            };

            Is2FAEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

            PopulateInputModel();
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

            if (Input.Avatar is not null)
            {
                // delete the old image and thumbnail
                _imageServices.Delete($"{user.ImageUrl}");
                _imageServices.Delete($"{user.ImageThumbnailUrl}");

                // upload the new image and thumbnail
                // generate a unique name for the image file 
                var imageName = $"{user.Id}{Path.GetExtension(Input.Avatar!.FileName)}";
                var (isUploaded, errorMessage) = await _imageServices.UploadAsync(Input.Avatar, imageName, "/images/users", hasThumbnail: true);

                if (!isUploaded)
                {
                    ModelState.AddModelError("Input.Avatar", errorMessage);
                    await LoadAsync(user);
                    return Page();
                }

                user.ImageUrl = $"/images/users/{imageName}";
                user.ImageThumbnailUrl = $"/images/users/thumb/{imageName}";
            }
            else if (Input.ImageRemoved)
            {
                _imageServices.Delete(user.ImageUrl);
                _imageServices.Delete(user.ImageThumbnailUrl);
                user.ImageUrl = null!;
                user.ImageThumbnailUrl = null!;
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            // check if phone number was changed
            if (Input.PhoneNumber != phoneNumber)
            {
                if (!string.IsNullOrWhiteSpace(Input.PhoneNumber))
                {
                    // check if phone number already exists for another user
                    var existingUser = await _userManager.Users
                        .Where(u => u.PhoneNumber == Input.PhoneNumber && u.Id != user.Id)
                        .FirstOrDefaultAsync();

                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Input.PhoneNumber", "This phone number is already used by another user.");
                        await LoadAsync(user);
                        return Page();
                    }
                }

                // now safe to update
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "ErrorAndDelete Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            // check if username was changed
            if (Input.Username != user.UserName)
            {
                // validate the uniqueness of the new username
                var hasError = await ValidateUserUniquenessAsync(Input.Username);
                if (hasError)
                {
                    ModelState.AddModelError("Input.Username", "Username is already taken.");
                    await LoadAsync(user);
                    return Page();
                }
                // now safe to update
                var setUserNameResult = await _userManager.SetUserNameAsync(user, Input.Username.Trim());
                if (!setUserNameResult.Succeeded)
                {
                    StatusMessage = "ErrorAndDelete Unexpected error when trying to set username.";
                    return RedirectToPage();
                }
            }

            if (Input.FullName != user.FullName)
                user.FullName = Input.FullName.Trim();

            if (Input.DateOfBirth != user.DateOfBirth)
                user.DateOfBirth = Input.DateOfBirth;

            if (Input.AreaId != user.AreaId)
                user.AreaId = Input.AreaId.Value;

            if (Input.GovernorateId != user.GovernorateId)
                user.GovernorateId = Input.GovernorateId.Value;

            if (Input.Address != user.Address)
                user.Address = Input.Address.Trim();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                StatusMessage = "ErrorAndDelete Unexpected error when updating user information.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "SuccessAndDelete Your profile has been updated successfully.";
            return RedirectToPage();
        }

        // a helper method to populate the InputModel with the governorates and areas dropdown lists
        private void PopulateInputModel()
        {
            var governorates = _context.Governorates.Where(g => !g.IsDeleted).OrderBy(g => g.Name).ToList();

            Input.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorates);

            // in case of editing, populate the areas dropdown list based on the selected governorate
            if (Input?.GovernorateId > 0)
            {
                var areas = _context.Areas
                    .Where(a => a.GovernorateId == Input.GovernorateId && !a.IsDeleted)
                    .OrderBy(a => a.Name)
                    .ToList();

                Input.Areas = _mapper.Map<IEnumerable<SelectListItem>>(areas);
            }
        }

        private async Task<bool> ValidateUserUniquenessAsync(string username)
        {
            var hasError = false;

            var existingUsernameUser = await _userManager.FindByNameAsync(username);
            if (existingUsernameUser != null)
                hasError = true;

            return hasError;
        }
    }
}