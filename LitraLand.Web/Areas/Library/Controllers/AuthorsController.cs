namespace LitraLand.Web.Areas.Library.Controllers
{
    [Area("Library")]
    [Authorize(Roles = AppRoles.Archive)]
    public class AuthorsController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AuthorsController(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var Authors = _context.Authors
                .AsNoTracking()
                .ToList();

            var viewModel = _mapper.Map<IEnumerable<AuthorViewModel>>(Authors);

            return View(viewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var author = _mapper.Map<Author>(model);

            author.CreatedById = User.GetUserId();

            _context.Authors.Add(author);
            _context.SaveChanges();

            var viewModel = _mapper.Map<AuthorViewModel>(author);

            return PartialView("_AuthorRow", viewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var Author = _context.Authors.Find(id);

            if (Author is null)
                return NotFound();

            var viewModel = _mapper.Map<AuthorFormViewModel>(Author);

            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var author = _context.Authors.Find(model.Id);

            if (author is null)
                return NotFound();

            _mapper.Map(model, author);
            author.LastUpdatedOn = DateTime.Now;
            author.LastUpdatedById = User.GetUserId();

            _context.SaveChanges();

            var viewModel = _mapper.Map<AuthorViewModel>(author);

            return PartialView("_AuthorRow", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var author = _context.Authors.Find(id);

            if (author is null)
                return NotFound("Author not found.");

            author.IsDeleted = !author.IsDeleted;
            author.LastUpdatedOn = DateTime.Now;
            author.LastUpdatedById = User.GetUserId();

            _context.SaveChanges();

            return Ok(new
            {
                message = "User status updated successfully.",
                lastUpdatedOn = author.LastUpdatedOn?.ToString("dd MMM yyyy hh:mm:ss tt")
            });
        }

        public IActionResult AllowItem(AuthorFormViewModel model)
        {
            var author = _context.Authors
                .IgnoreQueryFilters()
                .FirstOrDefault(a => a.Name == model.Name);

            var isAllowed = author is null || author.Id.Equals(model.Id);

            return Json(isAllowed);
        }
    }
}