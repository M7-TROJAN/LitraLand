using System.Linq.Dynamic.Core;
namespace LitraLand.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BooksController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageServices _imageServices;
        private readonly ICloudinaryService _cloudinaryService;
        public BooksController(ApplicationDbContext context, IMapper mapper,
            IWebHostEnvironment hostingEnvironment, IImageServices imageServices, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            _imageServices = imageServices;
            _cloudinaryService = cloudinaryService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetBooks()
        {
            var skip = int.TryParse(Request.Form["start"], out int s) ? s : 0; // Number of records to skip (if there is no vaue in the request, then skip 0 records)
            var pageSize = int.TryParse(Request.Form["length"], out int p) ? p : 10; // Number of records to take (10 is the default value)

            var searchValue = Request.Form["search[value]"].ToString(); // Search value from the search input field
            var sortColumnIndex = Request.Form["order[0][column]"];
            var sortColumn = Request.Form[$"columns[{sortColumnIndex}][name]"];
            var sortColumnDirection = Request.Form["order[0][dir]"];

            IQueryable<Book> books = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category);

            if (!string.IsNullOrEmpty(searchValue))
                books = books.Where(b => b.Title.Contains(searchValue) || b.Author!.Name.Contains(searchValue));

            books = books.OrderBy($"{sortColumn} {sortColumnDirection}"); // Using System.Linq.Dynamic.Core

            var data = await books.Skip(skip).Take(pageSize).ToListAsync();

            var mappedData = _mapper.Map<IEnumerable<BookViewModel>>(data);

            var recordsTotal = await books.CountAsync();

            var jsonData = new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mappedData };

            return Ok(jsonData);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var book = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Copies)
                .Include(b => b.Categories).ThenInclude(c => c.Category)
                .SingleOrDefault(b => b.Id == id);

            if (book is null)
                return NotFound();

            var ViewModel = _mapper.Map<BookViewModel>(book);

            return View(ViewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = populateViewModel();

            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = populateViewModel(model);

                return View("Form", model);
            }

            var book = _mapper.Map<Book>(model); // map the view model to the domain model

            if (model.Image is not null)
            {
                // begin upload the image to the server
                // generate a unique name for the image file 
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
                var uploadResult = await _imageServices.UploadAsync(model.Image, imageName, "/images/books", hasThumbnail: true);

                if (!uploadResult.isUploaded)
                {
                    ModelState.AddModelError(nameof(model.Image), uploadResult.errorMessage!);
                    return View("Form", populateViewModel(model));
                }

                book.ImageUrl = $"/images/books/{imageName}";
                book.ImageThumbnailUrl = $"/images/books/thumb/{imageName}";
                // end upload the image to the server

                // Begin upload the image to Cloudinary
                //var uploadResult = await _cloudinaryService.UploadImageAsync(model.Image, hasThumbnail: true);

                //if (!uploadResult.isUploaded)
                //{
                //    ModelState.AddModelError(nameof(model.Image), uploadResult.errorMessage!);
                //    return View("Form", populateViewModel(model));
                //}

                //book.ImageUrl = uploadResult.imageUrl;
                //book.ImageThumbnailUrl = uploadResult.thumbnailUrl;
                //book.ImagePublicId = uploadResult.publicId;
                // End upload the image to Cloudinary
            }

            book.CreatedById = User.GetUserId();

            _context.Books.Add(book);

            foreach (var categoryId in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory
                {
                    CategoryId = categoryId
                });
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = book.Id });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var book = _context.Books.Include(b => b.Categories).FirstOrDefault(b => b.Id == id);
            if (book is null)
                return NotFound();

            var model = _mapper.Map<BookFormViewModel>(book);

            var viewModel = populateViewModel(model); // to populate Categories and Authors dropdowns

            viewModel.SelectedCategories = book.Categories.Select(bc => bc.CategoryId).ToList(); // to pre-select categories in the dropdown

            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", populateViewModel(model));

            var book = _context.Books
                .Include(b => b.Categories)
                .Include(b => b.Copies)
                .FirstOrDefault(b => b.Id == model.Id);

            if (book is null)
                return NotFound();

            if (model.Image is not null)
            {
                // check if there is an old image and delete it
                if (!string.IsNullOrEmpty(book.ImageUrl))
                {
                    // delete the old image from the server
                    _imageServices.Delete(book.ImageUrl, book.ImageThumbnailUrl);

                    // delete the old image from Cloudinary
                    //if(!string.IsNullOrEmpty(book.ImagePublicId))
                    //    await _cloudinaryService.DeleteImageAsync(book.ImagePublicId);
                }

                // begin upload the image to the server
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";

                var uploadResult = await _imageServices.UploadAsync(model.Image, imageName, "/images/books", hasThumbnail: true);

                if (!uploadResult.isUploaded)
                {
                    ModelState.AddModelError(nameof(model.Image), uploadResult.errorMessage!);
                    return View("Form", populateViewModel(model));
                }

                model.ImageUrl = $"/images/books/{imageName}";
                model.ImageThumbnailUrl = $"/images/books/thumb/{imageName}";
                // end upload the image to the server

                // Begin upload the image to Cloudinary
                //var uploadResult = await _cloudinaryService.UploadImageAsync(model.Image, hasThumbnail: true);

                //if (!uploadResult.isUploaded)
                //{
                //    ModelState.AddModelError(nameof(model.Image), uploadResult.errorMessage!);
                //    return View("Form", populateViewModel(model));
                //}

                //model.ImageUrl = uploadResult.imageUrl;
                //model.ImageThumbnailUrl = uploadResult.thumbnailUrl;
                //model.ImagePublicId = uploadResult.publicId;
                // End upload the image to Cloudinary
            }
            else if (!string.IsNullOrEmpty(book.ImageUrl))
            {
                model.ImageUrl = book.ImageUrl; // keep the old image if no new image is uploaded
                model.ImageThumbnailUrl = book.ImageThumbnailUrl;
                model.ImagePublicId = book.ImagePublicId;
            }

            // update the book properties with the new values from the model
            book = _mapper.Map(model, book);

            // update the LastUpdatedOn property and the LastUpdatedById property
            book.LastUpdatedOn = DateTime.Now;
            book.LastUpdatedById = User.GetUserId();

            // update the book categories (note: BookCategories records related to the bookId will be deleted and re-inserted)
            foreach (var categoryId in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory { CategoryId = categoryId });
            }

            // update the book copies availability status if the IsAvailableForRental property of the original book is changed to false
            if (!model.IsAvailableForRental)
            {
                // if the book is not available for rental, then set the IsAvailableForRental property of all copies to false
                foreach (var copy in book.Copies)
                {
                    copy.IsAvailableForRental = false;
                }
            }

            // save the changes
            _context.Books.Update(book);
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = book.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var book = _context.Books.Find(id);
            if (book is null)
                return NotFound("Book not found.");

            book.IsDeleted = !book.IsDeleted;

            book.IsAvailableForRental = !book.IsDeleted ? book.IsAvailableForRental : false; // if the book is deleted, then it should not be available for rental

            book.LastUpdatedOn = DateTime.Now;
            book.LastUpdatedById = User.GetUserId();

            _context.SaveChanges();

            return Ok(new
            {
                message = "Book status updated successfully.",
                lastUpdatedOn = book.LastUpdatedOn?.ToString("dd MMM yyyy hh:mm:ss tt")
            });
        }

        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id) // physical delete (dangerous operation the book will be deleted permanently)
        {
            var book = _context.Books.Find(id);
            if (book is null)
                return NotFound("Book not found.");

            // first delete the bookCategories records if exists
            var bookCategories = _context.BookCategories.Where(bc => bc.BookId == id).ToList();

            if (bookCategories is not null)
                _context.BookCategories.RemoveRange(bookCategories);

            // second delete the image file if exists
            if (!string.IsNullOrEmpty(book.ImageUrl))
            {
                var imagePath = Path.Combine($"{_hostingEnvironment.WebRootPath}/images/books", book.ImageUrl);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            // finally delete the book record
            _context.Books.Remove(book);

            _context.SaveChanges();

            return Ok($"the book {book.Title} has been deleted successfully.");
        }
        */

        public IActionResult AllowItem(BookFormViewModel model)
        {
            // check if the book title is unique for the author
            // Example:
            // "Book 1", AuthorId: 1
            // "Book 2", AuthorId: 1
            // "Book 1", AuthorId: 1  -> not allowed (same title for the same author)

            var book = _context.Books.IgnoreQueryFilters().FirstOrDefault(b => b.Title == model.Title && b.AuthorId == model.AuthorId);

            var isAllowed = book is null || book.Id.Equals(model.Id);

            return Json(isAllowed);
        }

        private BookFormViewModel populateViewModel(BookFormViewModel? model = null)
        {
            var categories = _context.Categories.Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToList();
            var authors = _context.Authors.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();

            var viewModel = model ?? new BookFormViewModel();
            viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);
            viewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);

            return viewModel;
        }
    }
}