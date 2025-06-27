using HashidsNet;
using LitraLand.Domain.Entities.Community;

namespace LitraLand.Web.Areas.Community.Controllers
{
    [Area("Community")]
    [Authorize(Roles = $"{AppRoles.CommunityMember}, {AppRoles.CommunityAdmin}, {AppRoles.SuperAdmin}")]
    public class CommunityBooksController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageServices _imageServices;
        private readonly IHashids _hashids;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public CommunityBooksController(IApplicationDbContext context,
            IMapper mapper, IImageServices imageServices, IHashids hashids, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _imageServices = imageServices;
            _hashids = hashids;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Details(string key)
        {
            var decodedIds = _hashids.Decode(key);

            if (decodedIds.Length == 0)
                return NotFound();

            var bookId = decodedIds[0];

            if (bookId == 0)
                return NotFound();

            var book = _context.CommunityBooks
                .Include(b => b.Owner)
                .FirstOrDefault(b => b.Id == bookId);

            if (book is null)
                return NotFound();

            var viewModel = new CommunityBookViewModel
            {
                Key = key,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                ImageUrl = book.ImageUrl,
                ImageThumbnailUrl = book.ImageThumbnailUrl,
                Price = book.Price,
                IsForExchange = book.IsForExchange,
                CreatedOn = book.CreatedOn,
                OwnerId = book.OwnerId,
                OwnerUserName = book.Owner.UserName!,
                OwnerEmail = book.Owner.Email!,
                OwnerPhoneNumber = book.Owner.PhoneNumber
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new CommunityBookFormViewModel();

            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommunityBookFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            var book = _mapper.Map<CommunityBook>(model); // map the view model to the domain model

            if (model.Image is not null)
            {
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
                var uploadResult = await _imageServices.UploadAsync(model.Image, imageName, "/images/community/books", hasThumbnail: true);

                if (!uploadResult.isUploaded)
                {
                    ModelState.AddModelError(nameof(model.Image), uploadResult.errorMessage!);
                    return View("Form", model);
                }

                book.ImageUrl = $"/images/community/books/{imageName}";
                book.ImageThumbnailUrl = $"/images/community/books/thumb/{imageName}";
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

            book.OwnerId = User.GetUserId(); // set the owner of the book to the current user

            _context.CommunityBooks.Add(book); // add the book to the database

            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { key = _hashids.Encode(book.Id) });
        }

        [HttpGet]
        public IActionResult Edit(string key)
        {
            var decodedIds = _hashids.Decode(key);

            if (decodedIds.Length == 0)
                return NotFound();

            var bookId = decodedIds[0];

            if (bookId == 0)
                return NotFound();

            var book = _context.CommunityBooks
                .FirstOrDefault(b => b.Id == bookId);

            if (book is null)
                return NotFound();

            // Check if the current user is the owner of the book
            if (book.OwnerId != User.GetUserId())
            {
                return Forbid(); // User is not allowed to edit this book
            }

            var viewModel = _mapper.Map<CommunityBookFormViewModel>(book);

            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CommunityBookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var book = _context.CommunityBooks
                .FirstOrDefault(b => b.Id == model.Id);

            if (book is null)
                return NotFound();

            // Check if the current user is the owner of the book
            if (book.OwnerId != User.GetUserId())
                return Forbid();

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

                var uploadResult = await _imageServices.UploadAsync(model.Image, imageName, "/images/community/books", hasThumbnail: true);

                if (!uploadResult.isUploaded)
                {
                    ModelState.AddModelError(nameof(model.Image), uploadResult.errorMessage!);
                    return View("Form", model);
                }

                model.ImageUrl = $"/images/community/books/{imageName}";
                model.ImageThumbnailUrl = $"/images/community/books/thumb/{imageName}";
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

            // save the changes
            _context.CommunityBooks.Update(book);
            _context.SaveChanges();

            return RedirectToAction(actionName: nameof(Details),
                routeValues: new { key = _hashids.Encode(book.Id) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string key)
        {
            var decodedIds = _hashids.Decode(key);

            if (decodedIds.Length == 0 || decodedIds[0] == 0)
                return Json(new { success = false, message = "Invalid key provided." });

            var bookId = decodedIds[0];
            var book = _context.CommunityBooks.FirstOrDefault(b => b.Id == bookId);

            if (book is null)
                return Json(new { success = false, message = "Book not found." });

            // Ensure current user owns this book or is admin
            var currentUserId = User.GetUserId();
            if (book.OwnerId != currentUserId && !User.IsInRole(AppRoles.CommunityAdmin) && !User.IsInRole(AppRoles.SuperAdmin))
                return Json(new { success = false, message = "You do not have permission to delete this book." });

            // Delete images
            if (!string.IsNullOrWhiteSpace(book.ImageUrl))
                _imageServices.Delete(book.ImageUrl, book.ImageThumbnailUrl);

            // Delete DB record
            _context.CommunityBooks.Remove(book);
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = $"The book \"{book.Title}\" has been deleted successfully."
            });
        }
    }
}