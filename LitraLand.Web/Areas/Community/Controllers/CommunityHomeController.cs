using HashidsNet;
namespace LitraLand.Web.Areas.Community.Controllers
{
    [Area("Community")]
    [Authorize(Roles = $"{AppRoles.CommunityMember}, {AppRoles.CommunityAdmin}, {AppRoles.SuperAdmin}")]
    public class CommunityHomeController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHashids _hashids;

        public CommunityHomeController(IApplicationDbContext context, IMapper mapper, IHashids hashids)
        {
            _context = context;
            _mapper = mapper;
            _hashids = hashids;
        }

        public IActionResult Index()
        {
            var numberOfBooks = _context.CommunityBooks.Count();

            var numberOfMembers = (from user in _context.ApplicationUsers
                                   join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                   join role in _context.Roles on userRole.RoleId equals role.Id
                                   where role.Name == AppRoles.CommunityMember || role.Name == AppRoles.CommunityAdmin
                                   select user).Distinct().Count();

            //var topMembers = _context.ApplicationUsers
            //    .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
            //    .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, RoleName = r.Name })
            //    .Where(x => x.RoleName == AppRoles.CommunityMember || x.RoleName == AppRoles.CommunityAdmin)
            //    .GroupJoin(_context.CommunityBooks, x => x.u.Id, b => b.OwnerId, (x, books) => new
            //    {
            //        x.u.Id,
            //        x.u.UserName,
            //        x.u.ImageThumbnailUrl,
            //        BookCount = books.Count()
            //    })
            //    .OrderByDescending(x => x.BookCount)
            //    .Take(6)
            //    .Select(member => new TopMemberViewModel
            //    {
            //        UserId = member.Id,
            //        UserName = member.UserName,
            //        NumberOfBooks = member.BookCount,
            //        ImageUrl = member.ImageThumbnailUrl
            //    })
            //    .ToList();

            var topMembers = _context.TopMembersDto
                .FromSqlRaw("EXEC GetTopCommunityMembers @TopN = {0}", 6)
                .AsNoTracking()
                .ToList();


            var viewModel = new CommunityHomeViewModel
            {
                NumberOfBooks = numberOfBooks,
                NumberOfMembers = numberOfMembers,
                TopMembers = _mapper.Map<List<TopMemberViewModel>>(topMembers)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoadBooks()
        {
            var page = int.TryParse(Request.Form["page"], out int p) ? p : 1;
            var pageSize = int.TryParse(Request.Form["pageSize"], out int ps) ? ps : 10;
            var searchValue = Request.Form["searchTerm"].ToString().ToLower();
            var filterType = Request.Form["filter"].ToString().ToLower();

            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid paging parameters.");

            var query = _context.CommunityBooks
                .Include(b => b.Owner)
                .AsQueryable();

            // Apply search
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(b =>
                    b.Title.ToLower().Contains(searchValue) ||
                    b.Author.ToLower().Contains(searchValue) ||
                    (b.Price.ToString().Contains(searchValue))
                );
            }

            // Apply filter
            if (!string.IsNullOrEmpty(filterType))
            {
                if (filterType == "exchange")
                    query = query.Where(b => b.IsForExchange);
            }

            var totalRecords = await query.CountAsync();

            var books = await query
                .OrderByDescending(b => b.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new
                {
                    Key = _hashids.Encode(b.Id),
                    b.Title,
                    b.Author,
                    b.ImageThumbnailUrl,
                    b.Price,
                    b.IsForExchange,
                    CreatedOn = b.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss"),
                    b.OwnerId,
                    OwnerUserName = b.Owner.UserName!
                })
                .ToListAsync();

            return Json(new
            {
                books,
                totalRecords,
                hasMore = (page * pageSize) < totalRecords
            });
        }

    }
}