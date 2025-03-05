using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;

namespace LitraLand.Web.Controllers
{
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UsersController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IWebHostEnvironment webHostEnvironment,
            IMapper mapper,
            IEmailBodyBuilder emailBodyBuilder)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
            _emailBodyBuilder = emailBodyBuilder;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var usersViewModel = _mapper.Map<IEnumerable<UserViewModel>>(users);
            return View(usersViewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.Where(r => r.Name != AppRoles.User) // Exclude User role
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToListAsync();

            // Exclude SuperAdmin role if the current user is not a SuperAdmin
            if (!User.IsInRole(AppRoles.SuperAdmin))
            {
                roles = roles.Where(r => r.Value != AppRoles.SuperAdmin).ToList();
            }

            var viewModel = new UserFormViewModel
            {
                Roles = roles
            };

            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            var result = await _userManager.CreateAsync(user, model.Password!);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                var errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
                return BadRequest(errors);
            }

            await _userManager.AddToRolesAsync(user, model.SelectedRoles);

            // start send email confirmation
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code},
                protocol: Request.Scheme);
            
            var body = _emailBodyBuilder.GetEmailBody(
                "https://res.cloudinary.com/trojan74/image/upload/v1740774488/icon-positive-vote-1_qrtznr.svg",
                $"Hey {user.UserName}, thanks for joining us!",
                "Please click the link below to verify your email address.",
                HtmlEncoder.Default.Encode(callbackUrl!),
                "Verify Email");

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email", body);
            // end send email confirmation

            var viweModel = _mapper.Map<UserViewModel>(user);
            return PartialView("_UserRow", viweModel);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(string id)
        {
            var targetUser = await _userManager.FindByIdAsync(id);

            if (targetUser is null)
                return NotFound("User not found.");

            // prevent none super admin from editing super admin
            // Get the ID of the currently logged-in user
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the current logged-in user is a Super Admin
            var isCurrentUserSuperAdmin = User.IsInRole(AppRoles.SuperAdmin);

            // Get all roles assigned to the target user
            var targetUserRoles = await _userManager.GetRolesAsync(targetUser);

            // Check if the target user is a Super Admin
            var isTargetUserSuperAdmin = targetUserRoles.Contains(AppRoles.SuperAdmin);

            // Check if the current user is not a Super Admin and the target user is a Super Admin
            if (!isCurrentUserSuperAdmin && isTargetUserSuperAdmin)
                return BadRequest("You can't edit a super admin.");

            // populate roles dropdown list
            var roles = await _roleManager.Roles.Where(r => r.Name != AppRoles.User) // Exclude User role
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToListAsync();

            // Exclude SuperAdmin role from the dropdown list if the current user is not a SuperAdmin
            if (!isCurrentUserSuperAdmin)
            {
                roles = roles.Where(r => r.Value != AppRoles.SuperAdmin).ToList();
            }

            // Create the view model for the user form
            var viewModel = new UserFormViewModel
            {
                Id = targetUser.Id,
                UserName = targetUser.UserName!,
                Email = targetUser.Email!,
                FullName = targetUser.FullName,
                Roles = roles,
                SelectedRoles = targetUserRoles
            };

            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(model.Id!);

            if (user is null)
                return NotFound("User not found.");

            user = _mapper.Map(model, user);
            user.LastUpdatedOn = DateTime.Now;
            user.LastUpdatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // now we need to check if the roles have changed

                // first get the current roles in the database
                var currentRoles = await _userManager.GetRolesAsync(user);

                // check if the roles have changed by comparing the current roles with the selected roles
                var rolesUpdated = !currentRoles.SequenceEqual(model.SelectedRoles);

                // if the roles have changed, update the roles, otherwise do nothing
                if (rolesUpdated)
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                }

                // enforce security stamp update to force re-authentication (meaning the user will be logged out)
                await _userManager.UpdateSecurityStampAsync(user);

                var viweModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", viweModel);
            }

            return BadRequest(user.UserName + " " + string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var targetUser = await _userManager.FindByIdAsync(id);

            if (targetUser is null)
                return NotFound("User not found.");

            // Get the ID of the currently logged-in user
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the current logged-in user is a Super Admin
            var isCurrentUserSuperAdmin = User.IsInRole(AppRoles.SuperAdmin);

            // Get all roles assigned to the target user
            var targetUserRoles = await _userManager.GetRolesAsync(targetUser);

            // Check if the user to be deleted is a Super Admin
            var isTargetUserSuperAdmin = targetUserRoles.Contains(AppRoles.SuperAdmin);

            // check if the current user is not a Super Admin and the target user is a Super Admin
            if (!isCurrentUserSuperAdmin && isTargetUserSuperAdmin)
                return BadRequest("You can't reset the password of a super admin.");

            var viewModel = new ResetPasswordFormViewModel { Id = targetUser.Id };

            return PartialView("_ResetPasswordForm", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user is null)
                return NotFound("User not found.");

            var CurrenthashedPassword = user.PasswordHash;

            await _userManager.RemovePasswordAsync(user);

            var result = await _userManager.AddPasswordAsync(user, model.Password);
            if (result.Succeeded)
            {
                user.LastUpdatedOn = DateTime.Now;
                user.LastUpdatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

                await _userManager.UpdateAsync(user);

                var viweModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", viweModel);
            }

            user.PasswordHash = CurrenthashedPassword; // Revert the password hash if the new password addition fails
            await _userManager.UpdateAsync(user);

            var errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));

            return BadRequest(errors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnLock(string id)
        {
            var targetUser = await _userManager.FindByIdAsync(id);
            if (targetUser is null)
                return NotFound("User not found.");

            // check if the user is already unlocked
            var isLocked = await _userManager.IsLockedOutAsync(targetUser);

            // if the user is already unlocked, return a BadRequest response
            if (!isLocked)
                return BadRequest("User is already unlocked.");

            var result = await _userManager.SetLockoutEndDateAsync(targetUser, null);
            if (result.Succeeded)
            {
                targetUser.LastUpdatedOn = DateTime.Now;
                targetUser.LastUpdatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Ok(new
                {
                    message = $"User {targetUser.UserName} has been unlocked.",
                    lastUpdatedOn = targetUser.LastUpdatedOn?.ToString("dd MMM yyyy hh:mm:ss tt"),
                });
            }
            var errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
            return BadRequest(errors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            // Find the user to be deleted by ID
            var targetUser = await _userManager.FindByIdAsync(id);

            // If the user doesn't exist, return a 404 Not Found response
            if (targetUser is null)
                return NotFound("User not found.");

            // Get the current logged-in user's ID and roles
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isCurrentUserSuperAdmin = User.IsInRole(AppRoles.SuperAdmin);

            // Prevent users from changing their own status
            if (targetUser.Id == currentUserId)
                return BadRequest("You cannot change your own status.");

            // Get the roles of the target user
            var userRoles = await _userManager.GetRolesAsync(targetUser);
            var isTargetUserSuperAdmin = userRoles.Contains(AppRoles.SuperAdmin);

            // Prevent Admins from modifying the status of a Super Admin
            if (!isCurrentUserSuperAdmin && isTargetUserSuperAdmin)
                return BadRequest("Admins cannot modify the status of a Super Admin.");

            // Toggle the user's status (active/deleted)
            targetUser.IsDeleted = !targetUser.IsDeleted;
            targetUser.LastUpdatedOn = DateTime.Now;
            targetUser.LastUpdatedById = currentUserId;

            // Attempt to update the user
            var result = await _userManager.UpdateAsync(targetUser);

            // If update succeeds, return the new status update time
            if (result.Succeeded)
            {
                if(targetUser.IsDeleted)
                    await _userManager.UpdateSecurityStampAsync(targetUser);

                return Ok(new
                {
                    message = "User status updated successfully. the user is now " + (targetUser.IsDeleted ? "deleted" : "active"),
                    lastUpdatedOn = targetUser.LastUpdatedOn?.ToString("dd MMM yyyy hh:mm:ss tt"),
                });
            }

            // If update fails, return an error response with detailed messages
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return BadRequest($"Failed to update user status: {errors}");
        }

        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            // Find the user to be deleted by ID
            var targetUser = await _userManager.FindByIdAsync(id);

            // If the targetUser doesn't exist, return a 404 Not Found response
            if (targetUser is null)
                return NotFound();

            // Get the ID of the currently logged-in user
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the current logged-in user is a Super Admin
            var isCurrentUserSuperAdmin = User.IsInRole(AppRoles.SuperAdmin);

            // Prevent users from deleting themselves
            if (targetUser.Id == currentUserId)
                return BadRequest("You can't delete yourself.");

            // Get all roles assigned to the target user
            var targetUserRoles = await _userManager.GetRolesAsync(targetUser);

            // Check if the user to be deleted is a Super Admin
            var isTargetUserSuperAdmin = targetUserRoles.Contains(AppRoles.SuperAdmin);

            if (isTargetUserSuperAdmin)
            {
                // Get the list of all Super Admins
                var superAdmins = await _userManager.GetUsersInRoleAsync(AppRoles.SuperAdmin);

                // If there is only one Super Admin left, prevent deletion
                if (superAdmins.Count == 1)
                    return BadRequest("You can't delete the last super admin.");

                // If the current user is not a Super Admin, prevent them from deleting a Super Admin
                if (!isCurrentUserSuperAdmin)
                    return BadRequest("You can't delete a super admin.");
            }

            // Attempt to delete the user
            var result = await _userManager.DeleteAsync(targetUser);

            // Return OK if deletion is successful, otherwise return a BadRequest with error messages
            return result.Succeeded
                ? Ok($"User {targetUser.UserName} deleted successfully.")
                : BadRequest(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
        }
        */

        public async Task<IActionResult> AllowUserName(UserFormViewModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);

            var isAllowed = user is null || user.Id.Equals(model.Id);

            return Json(isAllowed);

        }
        public async Task<IActionResult> AllowEmail(UserFormViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            var isAllowed = user is null || user.Id.Equals(model.Id);

            return Json(isAllowed);
        }
    }
}