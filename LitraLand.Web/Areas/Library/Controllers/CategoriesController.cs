namespace LitraLand.Web.Areas.Library.Controllers
{
    [Area("Library")]
    [Authorize(Roles = AppRoles.Archive)]
    public class CategoriesController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CategoriesController(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var categories = _context.Categories
                .AsNoTracking()
                .ToList();

            var viewModel = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);

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
        public IActionResult Create(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var category = _mapper.Map<Category>(model);

            category.CreatedById = User.GetUserId();

            _context.Categories.Add(category);
            _context.SaveChanges();

            var viewModel = _mapper.Map<CategoryViewModel>(category);

            return PartialView("_CategoryRow", viewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);

            if (category is null)
                return NotFound();

            var viewModel = _mapper.Map<CategoryFormViewModel>(category);

            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var category = _context.Categories.Find(model.Id);

            if (category is null)
                return NotFound();

            category = _mapper.Map(model, category); // this overload of Map will map the properties of model to the existing category object and not create a new one
            category.LastUpdatedOn = DateTime.Now;
            category.LastUpdatedById = User.GetUserId();
            _context.SaveChanges();

            var viewModel = _mapper.Map<CategoryViewModel>(category);

            return PartialView("_CategoryRow", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var category = _context.Categories.Find(id);

            if (category is null)
                return NotFound("Category not found");

            category.IsDeleted = !category.IsDeleted;
            category.LastUpdatedOn = DateTime.Now;
            category.LastUpdatedById = User.GetUserId();

            _context.SaveChanges();

            return Ok(new
            {
                message = "Category status updated successfully.",
                lastUpdatedOn = category.LastUpdatedOn?.ToString("dd MMM yyyy hh:mm:ss tt")
            });
        }

        public IActionResult AllowItem(CategoryFormViewModel model)
        {
            var category = _context.Categories
                .IgnoreQueryFilters()
                .FirstOrDefault(c => c.Name == model.Name);

            var isAllowed = category is null || category.Id.Equals(model.Id);

            return Json(isAllowed);

        }
    }
}