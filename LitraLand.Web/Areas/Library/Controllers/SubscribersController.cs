using Hangfire;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.CodeAnalysis;
namespace LitraLand.Web.Areas.Library.Controllers
{
    [Area("Library")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Reception)]
    public class SubscribersController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageServices _imageServices;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IDataProtector _dataProtector;
        private readonly IWhatsAppClient _whatsAppClient;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;

        public SubscribersController(
            IApplicationDbContext context,
            IDataProtectionProvider dataProtector,
            IMapper mapper,
            IWhatsAppClient whatsAppClient,
            IImageServices imageServices,
            ICloudinaryService cloudinaryService,
            IWebHostEnvironment webHostEnvironment,
            IEmailSender emailSender,
            IEmailBodyBuilder emailBodyBuilder)
        {
            _context = context;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
            _mapper = mapper;
            _whatsAppClient = whatsAppClient;
            _imageServices = imageServices;
            _cloudinaryService = cloudinaryService;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _emailBodyBuilder = emailBodyBuilder;
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

            return PartialView("_SearchResult", viewModel);
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

            // make empty iEnumerable to avoid null reference exception in the view if no data found
            if (!mappedData.Any())
                mappedData = new List<SubscriberSearchResultViewModel>();

            var jsonData = new { recordsFiltered = totalRecords, recordsTotal = totalRecords, data = mappedData };

            return Ok(jsonData);
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
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .FirstOrDefaultAsync(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriberViewModel>(subscriber);
            viewModel.Key = id; // keep the encrypted id to be used in the view
            return View(viewModel);
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
            if (!ModelState.IsValid)
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

            subscriber.CreatedById = User.GetUserId();

            var subscription = new Subscription
            {
                CreatedById = subscriber.CreatedById,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1)
            };
            subscriber.Subscriptions.Add(subscription);

            _context.Subscribers.Add(subscriber);
            await _context.SaveChangesAsync();

            //Send welcome email
            var placeholders = new Dictionary<string, string>()
            {
                { "mediaUrl", "https://res.cloudinary.com/trojan74/image/upload/v1740774488/icon-positive-vote-1_qrtznr.svg" },
                { "header", $"Welcome {model.FirstName}," },
                { "body", "thanks for joining LitraLand! We're excited to have you 🤩<br>" +
                "Feel free to explore our features and let us know if you have any questions👌." }
            };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

            // use Hangfire to send the email in the background
            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
                model.Email,
                "Welcome to LitraLand",
                body
            ));

            //if the subscriber has WhatsApp, Send welcome message using WhatsApp
            if (model.HasWhatsApp)
            {
                var components = new List<WhatsAppComponent>()
                {
                    new WhatsAppComponent
                    {
                        Type = "body",
                        Parameters = new List<object>()
                        {
                            new WhatsAppTextParameter { Text = model.FirstName }
                        }
                    }
                };

                var phoneNumber = _webHostEnvironment.IsDevelopment() ? "01129816608" : model.PhoneNumber;

                // use Hangfire to send the WhatsApp message in the background
                BackgroundJob.Enqueue(() => _whatsAppClient.SendMessage(
                    $"2{phoneNumber}",
                    WhatsAppLanguageCode.English,
                    WhatsAppTemplates.WelcomeMessage,
                    components
                ));
            }

            var subscriberId = _dataProtector.Protect(subscriber.Id.ToString()); // encrypt the subscriber id to be used in the url

            return RedirectToAction(nameof(Details), new { id = subscriberId });
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
            subscriber.LastUpdatedById = User.GetUserId();
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = model.Key });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenewSubscription(string sKey)
        {
            // check if the key is sent in the request
            if (string.IsNullOrWhiteSpace(sKey))
                return BadRequest("Invalid request: Key is missing.");

            // decrypt the subscriber id to use it in the comparison
            if (!int.TryParse(TryUnprotect(sKey), out var subscriberId))
                return BadRequest("Invalid request: Subscriber Key is not valid.");

            var subscriber = await _context.Subscribers
                                    .Include(s => s.Subscriptions)
                                    .SingleOrDefaultAsync(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            if (subscriber.IsBlackListed)
                return BadRequest("This subscriber is blacklisted.");

            var lastSubscription = subscriber.Subscriptions.Last();

            var startDate = lastSubscription.EndDate < DateTime.Today ? DateTime.Today : lastSubscription.EndDate.AddDays(1);

            var newSubscription = new Subscription
            {
                CreatedById = User.GetUserId(),
                StartDate = startDate,
                EndDate = startDate.AddYears(1)
            };
            subscriber.Subscriptions.Add(newSubscription);
            await _context.SaveChangesAsync();


            // send email to notify the subscriber about the renewal
            var placeholders = new Dictionary<string, string>()
            {
                { "mediaUrl", "https://res.cloudinary.com/trojan74/image/upload/v1740874707/icon-positive-vote-2_sgflmb.svg" },
                { "header", $"Hello {subscriber.FirstName}," },
                { "body", $"We're happy to inform you that your subscription has been renewed for another year, <br>" +
                $"Beginning from {newSubscription.StartDate.ToString("dd MMM, yyyy")} to {newSubscription.EndDate.ToString("dd MMM, yyyy")}.🎉🎉<br>" +
                $"Enjoy our services and let us know if you have any questions." }
            };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
                subscriber.Email,
                "Subscription Renewal",
                body
            ));

            // send WhatsApp message to notify the subscriber about the renewal
            if (subscriber.HasWhatsApp)
            {
                var components = new List<WhatsAppComponent>()
                {
                    new WhatsAppComponent
                    {
                        Type = "body",
                        Parameters = new List<object>()
                        {
                            new WhatsAppTextParameter { Text = subscriber.FirstName },
                            new WhatsAppTextParameter { Text = newSubscription.StartDate.ToString("dd MMM, yyyy") },
                            new WhatsAppTextParameter { Text = newSubscription.EndDate.ToString("dd MMM, yyyy") }
                        }
                    }
                };
                var phoneNumber = _webHostEnvironment.IsDevelopment() ? "01129816608" : subscriber.PhoneNumber;

                BackgroundJob.Enqueue(() => _whatsAppClient.SendMessage(
                    $"2{phoneNumber}",
                    WhatsAppLanguageCode.English_US,
                    WhatsAppTemplates.SubscriptionRenewal,
                    components
                ));
            }

            var subscriptionModel = _mapper.Map<SubscriptionViewModel>(newSubscription);
            return PartialView("_SubscriptionRow", subscriptionModel);
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
                .SingleOrDefaultAsync(s => s.Email.Equals(model.Email));

            int subscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key)); // decrypt the subscriber id to use it in the comparison

            var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);

            return Json(isAllowed);
        }

        public async Task<IActionResult> AllowNationalId(SubscriberFormViewModel model)
        {
            var subscriber = await _context.Subscribers
                .SingleOrDefaultAsync(s => s.NationalId.Equals(model.NationalId));

            int subscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key)); // decrypt the subscriber id to use it in the comparison

            var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);
            return Json(isAllowed);
        }

        public async Task<IActionResult> AllowPhoneNumber(SubscriberFormViewModel model)
        {
            var subscriber = await _context.Subscribers
                .SingleOrDefaultAsync(s => s.PhoneNumber.Equals(model.PhoneNumber));

            int subscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key)); // decrypt the subscriber id to use it in the comparison

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