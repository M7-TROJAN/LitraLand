using CloudinaryDotNet;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace LitraLand.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;

        private readonly List<string> AllowedImageExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
        private readonly long MaxImageSize = 2 * 1024 * 1024; // 2097152 bytes (2MB)

        public BooksController(ApplicationDbContext context, IMapper mapper,
            IWebHostEnvironment hostingEnvironment, IOptions<CloudinarySettings> cloudinary)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            Account account = new Account
            {
                Cloud = cloudinary.Value.Cloud,
                ApiKey = cloudinary.Value.ApiKey,
                ApiSecret = cloudinary.Value.ApiSecret
            };
            _cloudinary = new Cloudinary(account);
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

            if (model.Image is not null)
            {
                var imageExtension = Path.GetExtension(model.Image.FileName);
                if (!AllowedImageExtensions.Contains(imageExtension))
                {
                    ModelState.AddModelError(nameof(model.Image), $"Invalid image format. Only {string.Join(", ", AllowedImageExtensions)} are allowed.");
                    model = populateViewModel(model);

                    return View("Form", model);
                }

                if (model.Image.Length > MaxImageSize)
                {
                    ModelState.AddModelError("Image", $"Image size should not exceed {MaxImageSize / 1024 / 1024}MB.");
                    model = populateViewModel(model);

                    return View("Form", model);
                }

                // begin to uoload the image to the server
                var imageName = $"{Guid.NewGuid()}{imageExtension}"; // make the image name unique

                var path = Path.Combine($"{_hostingEnvironment.WebRootPath}/images/books", imageName); // wwwroot/images/books/imageName
                var thumbPath = Path.Combine($"{_hostingEnvironment.WebRootPath}/images/books/thumb", imageName); // wwwroot/images/books/thumb/imageName

                using var stream = System.IO.File.Create(path); // create the image file
                await model.Image.CopyToAsync(stream); // copy the image to the file
                stream.Dispose(); // close the stream

                model.ImageUrl = $"/images/books/{imageName}"; 
                model.ImageThumbnailUrl = $"/images/books/thumb/{imageName}";

                // to resize the image for the thumbnail
                using var image = Image.Load(model.Image.OpenReadStream()); // use ImageSharp package to load the image
                var newWidth = 200; // the new width of the thumbnail image
                var ratio = (float)image.Width / newWidth; // calculate the ratio (ratio means the width of the image divided by the new width you want)
                var height = image.Height / ratio; // calculate the new height based on the ratio (example: if the image width is 400 and the new width is 200, then the ratio is 400/200 = 2, then the new height is height/2)
                image.Mutate(i => i.Resize(width: newWidth, height: (int)height)); // resize the image
                image.Save(thumbPath); // save the thumbnail image
                // end uoload the image to the server

                // to upload the image to Cloudinary 
                //using var stream = model.Image.OpenReadStream();
                //var imageName = $"{Guid.NewGuid()}{imageExtension}";
                //var imageParams = new ImageUploadParams
                //{
                //    File = new FileDescription(imageName, stream),
                //    UseFilename = true, // to use the same file name as the uploaded file
                //};

                //var uploadResult = await _cloudinary.UploadAsync(imageParams);

                //if (uploadResult.Error != null)
                //{
                //    ModelState.AddModelError("Image", "An error occurred while uploading the image.");

                //    var error = uploadResult.Error.Message;

                //    model = populateViewModel(model);

                //    return View("Form", model);
                //}
                //model.ImageUrl = uploadResult.SecureUrl.ToString();
                //model.ImageThumbnailUrl = GetThumbnailImageUrl(model.ImageUrl);
                //model.ImagePublicId = uploadResult.PublicId;
            }

            var book = _mapper.Map<Book>(model); // map the view model to the domain model

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

            var book = _context.Books.Include(b => b.Categories).FirstOrDefault(b => b.Id == model.Id);

            if (book is null)
                return NotFound();

            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(book.ImageUrl)/*&& !string.IsNullOrEmpty(book.ImagePublicId)*/)
                {
                    // delete the old image file if exists in the server
                    var oldImagePath = $"{_hostingEnvironment.WebRootPath}{book.ImageUrl}";
                    var oldThumbPath = $"{_hostingEnvironment.WebRootPath}{book.ImageThumbnailUrl}";

                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);

                    if (System.IO.File.Exists(oldThumbPath))
                        System.IO.File.Delete(oldThumbPath);

                    // delete the old image from Cloudinary
                    //await _cloudinary.DeleteResourcesAsync(new DelResParams
                    //{
                    //    PublicIds = new List<string> { book.ImagePublicId }
                    //});

                    //book.ImageUrl = null;
                    //book.ImageThumbnailUrl = null;
                    //book.ImagePublicId = null;
                }

                var imageExtension = Path.GetExtension(model.Image.FileName);
                if (!AllowedImageExtensions.Contains(imageExtension))
                {
                    ModelState.AddModelError(nameof(model.Image), $"Invalid image format. Only {string.Join(", ", AllowedImageExtensions)} are allowed.");
                    model = populateViewModel(model);

                    return View("Form", model);
                }

                if (model.Image.Length > MaxImageSize)
                {
                    ModelState.AddModelError("Image", "Image size should not exceed 2MB.");
                    model = populateViewModel(model);

                    return View("Form", model);
                }

                // begin uoload the image to the server
                var imageName = $"{Guid.NewGuid()}{imageExtension}"; // make the image name unique

                var path = Path.Combine($"{_hostingEnvironment.WebRootPath}/images/books", imageName); // wwwroot/images/books/imageName
                var thumbPath = Path.Combine($"{_hostingEnvironment.WebRootPath}/images/books/thumb", imageName); // wwwroot/images/books/thumb/imageName

                using var stream = System.IO.File.Create(path); // create the image file
                await model.Image.CopyToAsync(stream); // copy the image to the file
                stream.Dispose(); // close the stream

                model.ImageUrl = $"/images/books/{imageName}";
                model.ImageThumbnailUrl = $"/images/books/thumb/{imageName}";

                using var image = Image.Load(model.Image.OpenReadStream()); // use ImageSharp package to load the image
                var ratio = (float)image.Width / 200; // calculate the ratio (ratio means the width of the image divided by the new width you want)
                var height = image.Height / ratio; // calculate the new height based on the ratio
                image.Mutate(i => i.Resize(width: 200, height: (int)height)); // resize the image
                image.Save(thumbPath); // save the thumbnail image
                // End uoload the image to the server

                // Begin upload the image to Cloudinary
                //using var stream = model.Image.OpenReadStream();
                //var imageName = $"{Guid.NewGuid()}{imageExtension}";
                //var imageParams = new ImageUploadParams
                //{
                //    File = new FileDescription(imageName, stream),
                //    UseFilename = true, // to use the same file name as the uploaded file
                //};

                //var uploadResult = await _cloudinary.UploadAsync(imageParams);
                //if (uploadResult.Error != null)
                //{
                //    ModelState.AddModelError("Image", "An error occurred while uploading the image.");

                //    var error = uploadResult.Error.Message;

                //    model = populateViewModel(model);

                //    return View("Form", model);
                //}

                //model.ImageUrl = uploadResult.SecureUrl.ToString();
                //model.ImageThumbnailUrl = GetThumbnailImageUrl(model.ImageUrl);
                //model.ImagePublicId = uploadResult.PublicId;
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

            // update the LastUpdatedOn property
            book.LastUpdatedOn = DateTime.Now;

            // update the book categories (note: BookCategories records related to the bookId will be deleted and re-inserted)
            foreach (var categoryId in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory { CategoryId = categoryId });
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
                return NotFound();

            book.IsDeleted = !book.IsDeleted;

            book.IsAvailableForRental = !book.IsDeleted ? book.IsAvailableForRental : false; // if the book is deleted, then it should not be available for rental

            book.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id) // physical delete (dangerous operation the book will be deleted permanently)
        {
            var book = _context.Books.Find(id);
            if (book is null)
                return NotFound();

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

            return RedirectToAction(nameof(Index));
        }

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

        // to get the thumbnail image url from Cloudinary
        private string GetThumbnailImageUrl(string imageUrl)
        {
            var transformation = "c_thumb,w_200,g_face/";
            var separator = "image/upload/";
            var urlParts = imageUrl.Split(separator);

            var thumbnailImageUrl = $"{urlParts[0]}{separator}{transformation}{urlParts[1]}";

            return thumbnailImageUrl;
        }
    }
}