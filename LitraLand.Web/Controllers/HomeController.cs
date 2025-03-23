using HashidsNet;
using System.Diagnostics;

namespace LitraLand.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<HomeController> _logger;
        private readonly IHashids _hashids;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IMapper mapper, IHashids hashids)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _hashids = hashids;
        }

        public IActionResult Index()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction(nameof(Index), "Dashboard");

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
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
