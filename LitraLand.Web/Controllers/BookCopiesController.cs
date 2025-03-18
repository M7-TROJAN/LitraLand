namespace LitraLand.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BookCopiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BookCopiesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [AjaxOnly]
        public IActionResult Create(int bookId)
        {
            var book = _context.Books.Find(bookId);

            if (book is null)
                return NotFound();

            var viewModel = new BookCopyFormViewModel
            {
                BookId = bookId,
                ShowRentalInput = book.IsAvailableForRental
            };

            return PartialView("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var book = _context.Books.Find(model.BookId);

            if (book is null)
                return NotFound();

            var copy = new BookCopy
            {
                EditionNumber = model.EditionNumber,
                IsAvailableForRental = book.IsAvailableForRental ? model.IsAvailableForRental : false, // if book is not available for rental, copy should not be available for rental
                CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier),
            };

            book.Copies.Add(copy);

            _context.SaveChanges();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", viewModel);
        }

        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var copy = _context.BookCopies
                .Include(c => c.Book)
                .SingleOrDefault(c => c.Id == id);

            if (copy is null) return NotFound();

            var viewModel = _mapper.Map<BookCopyFormViewModel>(copy);
            viewModel.ShowRentalInput = copy.Book!.IsAvailableForRental;

            return PartialView("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = _context.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.Id == model.Id);

            if (copy is null)
                return NotFound();

            copy.EditionNumber = model.EditionNumber;
            copy.IsAvailableForRental = copy.Book!.IsAvailableForRental ? model.IsAvailableForRental : false; // if book is not available for rental, copy should not be available for rental
            copy.LastUpdatedOn = DateTime.Now;
            copy.LastUpdatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.SaveChanges();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", viewModel);
        }

        public IActionResult RentalHistory(int id)
        {
            var copyHistory = _context.RentalCopies
                .Include(c => c.Rental)
                .ThenInclude(r => r!.Subscriber)
                .Where(c => c.BookCopyId == id)
                .OrderByDescending(c => c.RentalDate)
                .ToList();

            if (copyHistory.Count == 0)
                return View("NoRentalHistory", Errors.NoRentalHistory);

            var viewModel = _mapper.Map<IEnumerable<CopyHistoryViewModel>>(copyHistory);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var copy = _context.BookCopies.Find(id);

            if (copy is null)
                return NotFound("Book copy not found");

            copy.IsDeleted = !copy.IsDeleted;

            copy.IsAvailableForRental = !copy.IsDeleted ? copy.IsAvailableForRental : false; // if book copy is deleted, it should not be available for rental

            copy.LastUpdatedOn = DateTime.Now;
            copy.LastUpdatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.SaveChanges();

            return Ok(new
            {
                message = "Book copy status updated successfully.",
                lastUpdatedOn = copy.LastUpdatedOn?.ToString("dd MMM yyyy hh:mm:ss tt")
            });
        }
    }
}