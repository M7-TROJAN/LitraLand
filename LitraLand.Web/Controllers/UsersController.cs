using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LitraLand.Web.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public UsersController(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var usersViewModel = _mapper.Map<IEnumerable<UserViewModel>>(users);
            return View(usersViewModel);
        }
    }
}
