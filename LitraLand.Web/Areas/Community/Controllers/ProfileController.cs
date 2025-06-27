using HashidsNet;

namespace LitraLand.Web.Areas.Community.Controllers
{
    [Area("Community")]
    [Authorize(Roles = $"{AppRoles.CommunityMember}, {AppRoles.CommunityAdmin}, {AppRoles.SuperAdmin}")]
    public class ProfileController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHashids _hashids;

        public ProfileController(IApplicationDbContext context, IMapper mapper, IHashids hashids)
        {
            _context = context;
            _mapper = mapper;
            _hashids = hashids;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();


            var user = await _context.ApplicationUsers
                .Include(u => u.Area)
                .Include(u => u.Governorate)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            var totalBooks = await _context.CommunityBooks
                .CountAsync(b => b.OwnerId == userId);

            var hasBooks = totalBooks > 0;

            var viewModel = new ProfileViewModel
            {
                Member = _mapper.Map<MemberViewModel>(user),
                TotalBooks = totalBooks,
                HasBooks = hasBooks
            };

            return View(viewModel);
        }

        public async Task<IActionResult> GetBooks(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid paging parameters.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var books = await _context.CommunityBooks
                .Where(b => b.OwnerId == userId)
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

            return PartialView("_CommunityBookCard", books);
        }
    }
}