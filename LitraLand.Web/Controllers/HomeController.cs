using HashidsNet;
using Microsoft.AspNetCore.WebUtilities;

namespace LitraLand.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<HomeController> _logger;
        private readonly IHashids _hashids;

        public HomeController(ILogger<HomeController> logger, IApplicationDbContext context, IMapper mapper, IHashids hashids)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _hashids = hashids;
        }

        public IActionResult Index()
        {
            if (User.Identity!.IsAuthenticated)
            {
                if (User.IsCommunityMember() || User.IsCommunityAdmin())
                    return RedirectToAction(nameof(Index), "CommunityHome", new { area = "Community" });
                else
                    return RedirectToAction(nameof(Index), "Dashboard", new { area = "Library" });
            }

            var lastAddedBooks = _context.Books
                .Include(b => b.Author)
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.Id) // or b => b.CreatedOn
                .Take(10)
                .ToList();

            var viewModel = _mapper.Map<IEnumerable<BookViewModel>>(lastAddedBooks);

            foreach (var book in viewModel)
                book.Key = _hashids.Encode(book.Id);

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statusCode = 500)
        {
            return View(new ErrorViewModel { ErrorCode = statusCode, ErrorDescription = ReasonPhrases.GetReasonPhrase(statusCode) });
        }
    }
}