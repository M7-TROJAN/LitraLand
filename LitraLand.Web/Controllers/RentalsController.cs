using Hangfire;
using Microsoft.AspNetCore.DataProtection;

namespace LitraLand.Web.Controllers
{
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Reception)]
    public class RentalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataProtector _dataProtector;
        private readonly IWhatsAppClient _whatsAppClient;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;

        public RentalsController(ApplicationDbContext context,
            IMapper mapper,
            IDataProtectionProvider dataProtector,
            IWebHostEnvironment webHostEnvironment,
            IEmailSender emailSender,
            IEmailBodyBuilder emailBodyBuilder,
            IWhatsAppClient whatsAppClient)
        {
            _context = context;
            _mapper = mapper;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _emailBodyBuilder = emailBodyBuilder;
            _whatsAppClient = whatsAppClient;
        }
        public IActionResult Details(int id)
        {
            var rental = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(rc => rc.BookCopy)
                .ThenInclude(bc => bc.Book)
                .FirstOrDefault(r => r.Id == id);

            if (rental is null)
                return NotFound();

            var viewModel = _mapper.Map<RentalViewModel>(rental);

            return View(viewModel);
        }

        public IActionResult Create(string sKey)
        {
            // decrypt the subscriber key and check if it is valid
            if (!int.TryParse(TryUnprotect(sKey), out var subscriberId))
                return BadRequest();

            // get the subscriber with its subscriptions and rentals
            var subscriber = _context.Subscribers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == subscriberId);

            // check if the subscriber is not found
            if (subscriber is null)
                return NotFound();

            // validate the subscriber
            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber);

            if (!string.IsNullOrEmpty(errorMessage) || !maxAllowedCopies.HasValue)
                return View("NotAllowdRental", errorMessage);

            var viewModel = new RentalFormViewModel
            {
                SubscriberKey = sKey,
                MaxAllowedCopies = maxAllowedCopies
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // check if the selected copies are empty (هكر لئيم)
            if (model.SelectedCopies == null || !model.SelectedCopies.Any())
                return View("NotAllowdRental", "Please select at least one Book before creating a rental.");

            // decrypt the subscriber key and check if it is valid
            if (!int.TryParse(TryUnprotect(model.SubscriberKey), out var subscriberId))
                return View("NotAllowdRental", "Invalid subscriber key.");

            // get the subscriber with its subscriptions and rentals
            var subscriber = _context.Subscribers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .FirstOrDefault(s => s.Id == subscriberId);

            // check if the subscriber is not found
            if (subscriber is null)
                return NotFound();

            // validate the subscriber
            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber);

            if (!string.IsNullOrEmpty(errorMessage) || !maxAllowedCopies.HasValue)
                return View("NotAllowdRental", errorMessage);

            // check if the selected copies count is more than the allowed count
            if (model.SelectedCopies.Count > maxAllowedCopies)
                return View("NotAllowdRental", Errors.InvalidCopiesCount);

            // get the selected Books copies
            var selectedCopies = _context.BookCopies
                .Include(bc => bc.Book)
                .Include(bc => bc.Rentals)
                .Where(bc => model.SelectedCopies.Contains(bc.SerialNumber) && !bc.IsDeleted && !bc.Book!.IsDeleted)
                .ToList();

            if (selectedCopies is null || !selectedCopies.Any())
                return View("NotAllowdRental", "Copies selected are not found.");

            if (selectedCopies.Count != model.SelectedCopies.Count)
                return View("NotAllowdRental", "Some Books you selected are not found.");

            var currentSubscriberRentals = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(rc => rc.BookCopy)
                .Where(r => r.SubscriberId == subscriberId)
                .SelectMany(r => r.RentalCopies)
                .Where(rc => !rc.ReturnDate.HasValue)
                .Select(rc => rc.BookCopy!.BookId)
                .ToList();

            List<RentalCopy> copies = new List<RentalCopy>();

            foreach (var bookCopy in selectedCopies)
            {
                if (!bookCopy.IsAvailableForRental || !bookCopy.Book!.IsAvailableForRental)
                    return View("NotAllowdRental", Errors.NotAvilableRental);

                if (bookCopy.Rentals.Any(rc => !rc.ReturnDate.HasValue))
                    return View("NotAllowdRental", Errors.CopyIsInRental);

                if (currentSubscriberRentals.Any(bookId => bookId == bookCopy.BookId))
                    return View("NotAllowdRental", $"This subscriber has already rented a copy of '{bookCopy.Book!.Title}' book");


                copies.Add(new RentalCopy
                {
                    BookCopyId = bookCopy.Id
                });
            }

            var rental = new Rental
            {
                RentalCopies = copies,
                CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            subscriber.Rentals.Add(rental);

            _context.SaveChanges();

            // send an email and WhatsAppMessage
            SendRentalEmail(subscriber, rental, selectedCopies);
            SendRentalWhatsAppMessage(subscriber, rental, selectedCopies);

            // return the user to subscribers controller, details action (/Subscribers/Details/id)
            return RedirectToAction("Details", "Subscribers", new { id = model.SubscriberKey });
        }

        public IActionResult Edit(int id)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(RentalFormViewModel mode)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetCopyDetails(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (!int.TryParse(model.Value, out int serialNumber))
                return BadRequest(Errors.InvalidSerialNumber);

            var bookCopy = _context.BookCopies
                .Include(bc => bc.Book)
                .FirstOrDefault(bc => bc.SerialNumber == serialNumber && !bc.IsDeleted && !bc.Book!.IsDeleted);

            if (bookCopy is null)
                return NotFound(Errors.InvalidSerialNumber);

            if (!bookCopy.IsAvailableForRental || !bookCopy.Book!.IsAvailableForRental)
                return BadRequest(Errors.NotAvilableRental);

            //TODO: check if the copy is in rental
            var isCopyInRental = _context.RentalCopies.Any(rc => rc.BookCopyId == bookCopy.Id && rc.ReturnDate == null);

            if (isCopyInRental)
                return BadRequest(Errors.CopyIsInRental);

            var viewModel = _mapper.Map<BookCopyViewModel>(bookCopy);

            return PartialView("_CopyDetails", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsDeleted(int id)
        {
            var rental = _context.Rentals.Find(id);

            if (rental is null)
                return NotFound("Rental not found.");

            if (rental.CreatedOn.Date != DateTime.Today)
                return BadRequest("You can't delete a rental that is not created today.");

            rental.IsDeleted = true;

            rental.LastUpdatedOn = DateTime.Now;
            rental.LastUpdatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.SaveChanges();

            return Ok("Rental has been deleted successfully.");
        }

        // a helper method to validate the subscriber before creating a rental
        private (string errorMessage, int? maxAllowedCopies) ValidateSubscriber(Subscriber subscriber)
        {
            // check if the subscriber is blacklisted
            if (subscriber.IsBlackListed)
                return (errorMessage: Errors.BlackListedSubscriber, maxAllowedCopies: null);

            // check if the subscriber is inactive (i.e. the last subscription is ended)
            var lastSubscription = subscriber.Subscriptions.LastOrDefault();
            if (lastSubscription is null || lastSubscription.EndDate < DateTime.Today.AddDays((int)RentalsConfigurations.MaxRentalDuration))
                return (errorMessage: Errors.InactiveSubscriber, maxAllowedCopies: null);

            // check if the subscriber has reached the max number of rentals
            // calculate the current rentals count for the subscriber
            var currentRentalsCount = subscriber.Rentals
                .SelectMany(r => r.RentalCopies)
                .Count(rc => !rc.ReturnDate.HasValue);
            // calculate the available copies count that the subscriber can rent
            var availableCopiesCount = (int)RentalsConfigurations.MaxAllowedCopies - currentRentalsCount;
            if (availableCopiesCount.Equals(0))
                return (errorMessage: Errors.MaxCopiesReached, maxAllowedCopies: null);

            // if we reach here, then the subscriber is valid and can create a rental with the available copies count
            return (errorMessage: string.Empty, maxAllowedCopies: availableCopiesCount);
        }

        // a helper method to send an email to the subscriber after creating a rental
        private void SendRentalEmail(Subscriber subscriber, Rental rental, List<BookCopy> selectedCopies)
        {
            var placeholders = new Dictionary<string, string>
            {
                { "mediaUrl", "https://res.cloudinary.com/trojan74/image/upload/v1740774488/icon-positive-vote-1_qrtznr.svg" },
                { "header", $"Hey {subscriber.FirstName}," },
                { "body", $"Thanks for renting from LitraLand! We're excited to have you 🤩.<br><br>" +
                          $"Your rental has been successfully created with the following details:<br>" +
                          $"📅 <strong>Rental Date:</strong> {rental.StartDate.ToString("dd MMM, yyyy")}<br>" +
                          $"⏳ <strong>Rental Duration:</strong> {(int)RentalsConfigurations.MaxRentalDuration} days<br>" +
                          $"📚 <strong>Rented Books:</strong> {string.Join(", ", selectedCopies.Select(c => c.Book!.Title))}<br><br>" +
                          $"Please make sure to return the books on time to avoid any penalties.<br>" +
                          $"Enjoy reading! 📖😊" }
            };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
                subscriber.Email,
                "Rental Created Successfully",
                body
            ));
        }

        // a helper method to send a WhatsApp message to the subscriber after creating a rental
        private void SendRentalWhatsAppMessage(Subscriber subscriber, Rental rental, List<BookCopy> selectedCopies)
        {
            if (!subscriber.HasWhatsApp)
                return;

            var components = new List<WhatsAppComponent>
            {
                new WhatsAppComponent
                {
                    Type = "body",
                    Parameters = new List<object>
                    {
                        new WhatsAppTextParameter { Text = subscriber.FirstName },
                        new WhatsAppTextParameter { Text = rental.StartDate.ToString("dd MMM, yyyy")},
                        new WhatsAppTextParameter { Text = ((int)RentalsConfigurations.MaxRentalDuration).ToString() },
                        new WhatsAppTextParameter { Text = string.Join(", ", selectedCopies.Select(c => c.Book!.Title)) }
                    }
                }
            };

            var phoneNumber = _webHostEnvironment.IsDevelopment() ? "01129816608" : subscriber.PhoneNumber;

            BackgroundJob.Enqueue(() => _whatsAppClient.SendMessage(
                $"2{phoneNumber}",
                WhatsAppLanguageCode.English,
                WhatsAppTemplates.RentalSuccess,
                components
            ));
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