using Hangfire;
using HashidsNet;
using LitraLand.Domain.Entities.Common;
using Microsoft.AspNetCore.Identity;
namespace LitraLand.Web.Areas.Community.Controllers
{
    [Area("Community")]
    [Authorize(Roles = $"{AppRoles.CommunityMember}, {AppRoles.CommunityAdmin}, {AppRoles.SuperAdmin}")]
    public class MembersController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IMapper _mapper;
        private readonly IHashids _hashids;
        private List<string> _includedRoles = [AppRoles.CommunityAdmin, AppRoles.CommunityMember];


        public MembersController(IApplicationDbContext context, IMapper mapper, IHashids hashids,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender, IEmailBodyBuilder emailBodyBuilder)
        {
            _context = context;
            _mapper = mapper;
            _hashids = hashids;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _emailBodyBuilder = emailBodyBuilder;
        }

        public async Task<IActionResult> Index()
        {
            //var users = await _userManager.Users.ToListAsync();
            //var usersViewModel = _mapper.Map<IEnumerable<UserViewModel>>(users);
            //return View(usersViewModel);

            var includedRolesList = string.Join(",", _includedRoles.Select(role => $"'{role}'"));

            var query = $@"
                SELECT DISTINCT u.*
                FROM AspNetUsers u
                LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
                LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
                WHERE r.Name IS NULL OR r.Name IN ({includedRolesList})
            ";

            var users = await _context.ApplicationUsers
                .FromSqlRaw(query)
                .ToListAsync();

            var usersViewModel = _mapper.Map<IEnumerable<UserViewModel>>(users);

            return View(usersViewModel);
        }

        [Authorize(Roles = $"{AppRoles.CommunityAdmin}, {AppRoles.SuperAdmin}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string id)
        {
            var targetUser = await _userManager.FindByIdAsync(id);
            if (targetUser is null)
                return NotFound("User not found.");

            // check if the current user is trying to lock themselves
            var currentUserId = User.GetUserId();
            if (targetUser.Id == currentUserId)
                return BadRequest("You can't lock your own account.");

            // check if the user is trying to lock a super admin
            var isTargetUserSuperAdmin = await _userManager.IsInRoleAsync(targetUser, AppRoles.SuperAdmin);
            if (isTargetUserSuperAdmin)
                return BadRequest("You can't lock a super admin.");

            // check if the user is already locked
            var isLocked = await _userManager.IsLockedOutAsync(targetUser);
            if (isLocked)
                return BadRequest("User is already locked.");

            // lock the user indefinitely (can be set to a specific date if needed)
            var lockoutEndDate = DateTimeOffset.MaxValue;
            var result = await _userManager.SetLockoutEndDateAsync(targetUser, lockoutEndDate);

            if (!result.Succeeded)
            {
                var errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
                return BadRequest(errors);
            }

            targetUser.LastUpdatedOn = DateTime.Now;
            targetUser.LastUpdatedById = User.GetUserId();

            // send email notification
            var placeholders = new Dictionary<string, string>()
            {
                { "mediaUrl", "https://res.cloudinary.com/trojan74/image/upload/v1741402646/calendar_zfohjc_vdrflq.png" },
                { "header", $"Hello {targetUser.UserName}," },
                { "body", "Your account has been locked by the administrator." }
            };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
                targetUser.Email!,
                "Account Locked",
                body
            ));

            // Update the user's security stamp to force re-authentication
            await _userManager.UpdateSecurityStampAsync(targetUser);

            return Ok(new
            {
                message = $"User {targetUser.UserName} has been locked.",
                lastUpdatedOn = targetUser.LastUpdatedOn?.ToString("dd MMM yyyy hh:mm:ss tt"),
            });
        }


        [Authorize(Roles = $"{AppRoles.CommunityAdmin}, {AppRoles.SuperAdmin}")]
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
            if (!result.Succeeded)
            {
                var errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
                return BadRequest(errors);
            }

            targetUser.LastUpdatedOn = DateTime.Now;
            targetUser.LastUpdatedById = User.GetUserId();

            // send email notification
            var placeholders = new Dictionary<string, string>()
            {
                { "mediaUrl", "https://res.cloudinary.com/trojan74/image/upload/v1740774488/icon-positive-vote-1_qrtznr.svg" },
                { "header", $"Hey {targetUser.UserName}," },
                { "body", "Your account has been unlocked successfully." }
            };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
                targetUser.Email!,
                "Account Unlocked",
                body
            ));

            return Ok(new
            {
                message = $"User {targetUser.UserName} has been unlocked.",
                lastUpdatedOn = targetUser.LastUpdatedOn?.ToString("dd MMM yyyy hh:mm:ss tt"),
            });
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
            var currentUserId = User.GetUserId();
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
                if (targetUser.IsDeleted)
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


        // This action is for viewing other users' profiles
        public async Task<IActionResult> Profile(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Invalid User ID.");

            // Check if the user is trying to view his own profile
            var currentUserId = User.GetUserId();
            if (currentUserId.Equals(id, StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "Profile", new { area = "Community" });

            var user = await _context.ApplicationUsers
                .Include(u => u.Area)
                .Include(u => u.Governorate)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user is null)
                return NotFound("User not found.");

            var totalBooks = await _context.CommunityBooks
                .CountAsync(b => b.OwnerId == user.Id);

            var hasBooks = totalBooks > 0;

            var viewModel = new ProfileViewModel
            {
                Member = _mapper.Map<MemberViewModel>(user),
                TotalBooks = totalBooks,
                HasBooks = hasBooks
            };

            return View(viewModel);
        }

        public async Task<IActionResult> GetAllMemberBooks(string memberId, int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid paging parameters.");

            var books = await _context.CommunityBooks
                .Include(b => b.Owner)
                .Where(b => b.OwnerId == memberId)
                .OrderByDescending(b => b.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new CommunityBookViewModel
                {
                    Key = _hashids.Encode(b.Id),
                    Title = b.Title,
                    Author = b.Author,
                    Description = b.Description,
                    ImageUrl = b.ImageUrl,
                    ImageThumbnailUrl = b.ImageThumbnailUrl,
                    IsForExchange = b.IsForExchange,
                    Price = b.Price,
                    CreatedOn = b.CreatedOn
                })
                .ToListAsync();

            return PartialView("_CommunityMemberBookCard", books);
        }
    }
}