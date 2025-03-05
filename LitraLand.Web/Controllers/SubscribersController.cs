using LitraLand.Web.Views;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.CodeAnalysis;
namespace LitraLand.Web.Controllers
{
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Reception)]
    public class SubscribersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageServices _imageServices;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IDataProtector _dataProtector;

        public SubscribersController(
            ApplicationDbContext context,
            IDataProtectionProvider dataProtector,
            IMapper mapper,
            IImageServices imageServices,
            ICloudinaryService cloudinaryService)
        {
            _context = context;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
            _mapper = mapper;
            _imageServices = imageServices;
            _cloudinaryService = cloudinaryService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var subscriber = await _context.Subscribers
                .SingleOrDefaultAsync(s => s.NationalId == model.Value || s.PhoneNumber == model.Value || s.Email == model.Value);

            var viewModel = _mapper.Map<SubscriberSearchResultViewModel>(subscriber);

            if (subscriber is not null)
                viewModel.Key = _dataProtector.Protect(subscriber.Id.ToString()); // encrypt the subscriber id to be used in the url

            return PartialView("_Result", viewModel);
        }

        [Authorize(Roles = AppRoles.SuperAdmin)]
        public IActionResult SubscribersList()
        {
            return View();
        }

        [Authorize(Roles = AppRoles.SuperAdmin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetSubscribers()
        {
            var skip = int.TryParse(Request.Form["start"], out int s) ? s : 0;
            var pageSize = int.TryParse(Request.Form["length"], out int p) ? p : 10;
            var searchValue = Request.Form["searchTerm"].ToString().ToLower();

            IQueryable<Subscriber> subscribers = _context.Subscribers;

            if (!string.IsNullOrEmpty(searchValue))
            {
                subscribers = subscribers.Where(s =>
                    (s.FirstName + " " + s.LastName).ToLower().Contains(searchValue) || // search by full name ex "mahmoud mattar"
                    s.NationalId.Contains(searchValue) ||
                    s.PhoneNumber.Contains(searchValue) ||
                    s.Email.ToLower().Contains(searchValue)
                );
            }

            var totalRecords = await subscribers.CountAsync();

            var data = await subscribers
                .OrderBy(s => s.FirstName)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var mappedData = data.Select(subscriber => new SubscriberSearchResultViewModel
            {
                Key = _dataProtector.Protect(subscriber.Id.ToString()),
                FullName = $"{subscriber.FirstName} {subscriber.LastName}",
                ImageThumbnailUrl = subscriber.ImageThumbnailUrl,
                Email = subscriber.Email
            });

            // make empty iEnumerable to avoid null reference exception in the view
            if (!mappedData.Any())
                mappedData = new List<SubscriberSearchResultViewModel>();

            var jsonData = new { recordsFiltered = totalRecords, recordsTotal = totalRecords, data = mappedData };

            return Ok(jsonData);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = PopulateViewModel();
            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubscriberFormViewModel model)
        {
            if(!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var subscriber = _mapper.Map<Subscriber>(model);

            // begin upload the image to the server
            // generate a unique name for the image file 
            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image!.FileName)}";
            var path = @"/images/subscribers";
            var (isUploaded, errorMessage) = await _imageServices.UploadAsync(model.Image, imageName, path, hasThumbnail: true);

            if (!isUploaded)
            {
                ModelState.AddModelError(nameof(model.Image), errorMessage!);
                return View("Form", PopulateViewModel(model));
            }

            subscriber.ImageUrl = $"{path}/{imageName}";
            subscriber.ImageThumbnailUrl = $"{path}/thumb/{imageName}";
            // end upload the image to the server

            // begin upload the image to Cloudinary
            //var uploadResult = await _cloudinaryService.UploadImageAsync(model.Image, hasThumbnail: true);

            //if (!uploadResult.isUploaded)
            //{
            //    ModelState.AddModelError(nameof(model.Image), uploadResult.errorMessage!);
            //    var viewModel = populateViewModel(model);
            //    return View("Form", viewModel);
            //}

            //subscriber.ImageUrl = uploadResult.imageUrl!;
            //subscriber.ImageThumbnailUrl = uploadResult.thumbnailUrl!;
            //subscriber.ImagePublicId = uploadResult.publicId;
            // end upload the image to Cloudinary

            subscriber.CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.Subscribers.Add(subscriber);

            await _context.SaveChangesAsync();

            //TODO: send a welcome email to the subscriber

            var subscriberId = _dataProtector.Protect(subscriber.Id.ToString()); // encrypt the subscriber id to be used in the url

            return RedirectToAction(nameof(Details), new {id = subscriberId});
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            // check if the key is sent in the request
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Invalid request: Key is missing.");

            // decrypt the subscriber id to use it in the comparison
            if (!int.TryParse(TryUnprotect(id), out var subscriberId))
                return BadRequest("Invalid request: Subscriber Key is not valid.");

            var subscriber = await _context.Subscribers.FindAsync(subscriberId);
            if (subscriber is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriberFormViewModel>(subscriber);
            viewModel = PopulateViewModel(viewModel);
            viewModel.Key = id; // keep the encrypted id to be used in the view
            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            // check if the key is sent in the request
            if (string.IsNullOrWhiteSpace(model.Key))
                return BadRequest("Invalid request: Key is missing.");

            // decrypt the subscriber id to use it in the comparison
            if (!int.TryParse(TryUnprotect(model.Key), out var subscriberId))
                return BadRequest("Invalid request: Subscriber Key is not valid.");

            var subscriber = await _context.Subscribers.FindAsync(subscriberId);

            if (subscriber is null)
                return NotFound();

            if (model.Image is not null)
            {
                // begin upload the image to the server
                // delete the old image from the server
                _imageServices.Delete(subscriber.ImageUrl, subscriber.ImageThumbnailUrl);

                // generate a unique name for the image file 
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
                var path = @"/images/subscribers";
                var (isUploaded, errorMessage) = await _imageServices.UploadAsync(model.Image, imageName, path, hasThumbnail: true);
                if (!isUploaded)
                {
                    ModelState.AddModelError(nameof(model.Image), errorMessage!);
                    return View("Form", PopulateViewModel(model));
                }
                model.ImageUrl = $"{path}/{imageName}";
                model.ImageThumbnailUrl = $"{path}/thumb/{imageName}";
                // end upload the image to the server

                // begin upload the image to Cloudinary
                // delete the old image from the server
                //if (!string.IsNullOrEmpty(subscriber.ImagePublicId))
                //    await _cloudinaryService.DeleteImageAsync(subscriber.ImagePublicId);

                //var uploadResult = await _cloudinaryService.UploadImageAsync(model.Image, hasThumbnail: true);
                //if (!uploadResult.isUploaded)
                //{
                //    ModelState.AddModelError(nameof(model.Image), uploadResult.errorMessage!);
                //    var viewModel = PopulateViewModel(model);
                //    return View("Form", viewModel);
                //}
                //model.ImageUrl = uploadResult.imageUrl!;
                //model.ImageThumbnailUrl = uploadResult.thumbnailUrl!;
                //model.ImagePublicId = uploadResult.publicId;
                // end upload the image to Cloudinary
            }
            else if (!string.IsNullOrEmpty(subscriber.ImageUrl))
            {
                model.ImageUrl = subscriber.ImageUrl; // keep the old image if no new image is uploaded
                model.ImageThumbnailUrl = subscriber.ImageThumbnailUrl;
                model.ImagePublicId = subscriber.ImagePublicId;
            }

            _mapper.Map(model, subscriber);

            subscriber.LastUpdatedOn = DateTime.Now;
            subscriber.LastUpdatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = model.Key });
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            // check if the key is sent in the request
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Invalid request: Key is missing.");

            // decrypt the subscriber id to use it in the comparison
            if (!int.TryParse(TryUnprotect(id), out var subscriberId))
                return BadRequest("Invalid request: Subscriber Key is not valid.");

            var subscriber = await _context.Subscribers
                .Include(s => s.Area)
                .Include(s => s.Governorate)
                .FirstOrDefaultAsync(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriberViewModel>(subscriber);
            viewModel.Key = id; // keep the encrypted id to be used in the view
            return View(viewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult GetAreas(int governorateId)
        {
            var areas = _context.Areas
                .Where(a => a.GovernorateId == governorateId && !a.IsDeleted)
                .OrderBy(a => a.Name)
                .ToList();

            return Ok(_mapper.Map<IEnumerable<SelectListItem>>(areas));
        }

        public async Task<IActionResult> AllowEmail(SubscriberFormViewModel model)
        {
            var subscriber = await _context.Subscribers
                .FirstOrDefaultAsync(s => s.Email.Equals(model.Email));

            int subscriberId;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key)); // decrypt the subscriber id to use it in the comparison
            else
                subscriberId = 0;

            var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);

            return Json(isAllowed);
        }

        public async Task<IActionResult> AllowNationalId(SubscriberFormViewModel model)
        {
            var subscriber = await _context.Subscribers
                .FirstOrDefaultAsync(s => s.NationalId.Equals(model.NationalId));

            int subscriberId;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key)); // decrypt the subscriber id to use it in the comparison
            else
                subscriberId = 0;

            var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);
            return Json(isAllowed);
        }

        public async Task<IActionResult> AllowPhoneNumber(SubscriberFormViewModel model)
        {
            var subscriber = await _context.Subscribers
                .FirstOrDefaultAsync(s => s.PhoneNumber.Equals(model.PhoneNumber));

            int subscriberId;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key)); // decrypt the subscriber id to use it in the comparison
            else
                subscriberId = 0;

            var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);
            return Json(isAllowed);
        }

        // a helper method to populate the view model with the governorates and areas dropdown lists
        private SubscriberFormViewModel PopulateViewModel(SubscriberFormViewModel? model = null)
        {
            var viewModel = model ?? new SubscriberFormViewModel();

            var governorates = _context.Governorates.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();

            viewModel.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorates);

            // in case of editing, populate the areas dropdown list based on the selected governorate
            if (model?.GovernorateId > 0)
            {
                var areas = _context.Areas
                    .Where(a => a.GovernorateId == model.GovernorateId && !a.IsDeleted)
                    .OrderBy(a => a.Name)
                    .ToList();

                viewModel.Areas = _mapper.Map<IEnumerable<SelectListItem>>(areas);
            }

            return viewModel;
        }

        // a helper method to decrypt the protected value and return the decrypted value
        // we use it to avoid throwing an exception when the value is not valid
        private string? TryUnprotect(string protectedValue)
        {
            try
            {
                return _dataProtector.Unprotect(protectedValue);
            }
            catch
            {
                return null;
            }
        }
    }
}