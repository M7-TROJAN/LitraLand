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
                .ThenInclude(bc => bc!.Book)
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
                return View("NotAllowedRental", errorMessage);

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
                return View("NotAllowedRental", "Please select at least one Book before creating a rental.");

            // decrypt the subscriber key and check if it is valid
            if (!int.TryParse(TryUnprotect(model.SubscriberKey), out var subscriberId))
                return View("NotAllowedRental", "Invalid subscriber key.");

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
                return View("NotAllowedRental", errorMessage);

            // check if the selected copies count is more than the allowed count
            if (model.SelectedCopies.Count > maxAllowedCopies)
                return View("NotAllowedRental", Errors.InvalidCopiesCount);

            // get the selected Books copies
            var selectedCopies = _context.BookCopies
                .Include(bc => bc.Book)
                .Include(bc => bc.Rentals)
                .Where(bc => model.SelectedCopies.Contains(bc.SerialNumber) && !bc.IsDeleted && !bc.Book!.IsDeleted)
                .ToList();

            if (selectedCopies.Count != model.SelectedCopies.Count)
                return View("NotAllowedRental", "Some Books you selected are not found.");

            var (rentalsError, copies) = ValidateCopies(selectedCopies, subscriberId);

            if (!string.IsNullOrEmpty(rentalsError))
                return View("NotAllowedRental", rentalsError);

            var rental = new Rental
            {
                RentalCopies = copies!,
                CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            subscriber.Rentals.Add(rental);

            _context.SaveChanges();

            // send an email and WhatsAppMessage
            SendRentalConfirmationEmail(subscriber, rental, selectedCopies);
            SendRentalConfirmationWhatsAppMessage(subscriber, rental, selectedCopies);

            // return the user to subscribers controller, details action (/Subscribers/Details/id)
            //return RedirectToAction("Details", "Subscribers", new { id = model.SubscriberKey });

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }

        public IActionResult Edit(int id)
        {
            var rental = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(rc => rc.BookCopy)
                .FirstOrDefault(r => r.Id == id);

            // check if the rental is not found or not created today (to avoid editing rentals created in the past)
            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            var subscriber = _context.Subscribers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .FirstOrDefault(s => s.Id == rental.SubscriberId);

            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber!, rental.Id);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var currentCopiesIds = rental.RentalCopies.Select(rc => rc.BookCopyId).ToList();

            var currentCopies = _context.BookCopies
                .Where(bc => currentCopiesIds.Contains(bc.Id))
                .Include(bc => bc.Book)
                .ToList();

            var viewModel = new RentalFormViewModel
            {
                Id = id,
                SubscriberKey = _dataProtector.Protect(subscriber!.Id.ToString()),
                MaxAllowedCopies = maxAllowedCopies,
                CurrentCopies = _mapper.Map<IEnumerable<BookCopyViewModel>>(currentCopies)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // check if the selected copies are empty (هكر لئيم)
            if (model.SelectedCopies == null || !model.SelectedCopies.Any())
                return View("NotAllowedRental", "Please select at least one Book before creating a rental.");

            var rental = _context.Rentals
                .Include(r => r.RentalCopies)
                .FirstOrDefault(r => r.Id == model.Id);

            // check if the rental is not found or not created today (to avoid editing rentals created in the past)
            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            // decrypt the subscriber key and check if it is valid
            if (!int.TryParse(TryUnprotect(model.SubscriberKey), out var subscriberId))
                return View("NotAllowedRental", "Invalid subscriber key.");

            // get the subscriber with its subscriptions and rentals
            var subscriber = _context.Subscribers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .FirstOrDefault(s => s.Id == subscriberId);

            // validate the subscriber
            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber!, model.Id);

            if (!string.IsNullOrEmpty(errorMessage) || !maxAllowedCopies.HasValue)
                return View("NotAllowedRental", errorMessage);

            // check if the selected copies count is more than the allowed count
            if (model.SelectedCopies.Count > maxAllowedCopies)
                return View("NotAllowedRental", Errors.InvalidCopiesCount);

            // get the selected Books copies
            var selectedCopies = _context.BookCopies
                .Include(bc => bc.Book)
                .Include(bc => bc.Rentals)
                .Where(bc => model.SelectedCopies.Contains(bc.SerialNumber) && !bc.IsDeleted && !bc.Book!.IsDeleted)
                .ToList();

            if (selectedCopies.Count != model.SelectedCopies.Count)
                return View("NotAllowedRental", "Some Books you selected are not found.");

            var (rentalsError, copies) = ValidateCopies(selectedCopies, subscriberId, rental.Id);

            if (!string.IsNullOrEmpty(rentalsError))
                return View("NotAllowedRental", rentalsError);

            rental.RentalCopies = copies!;
            rental.LastUpdatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
            rental.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            // send an email and WhatsAppMessage
            SendRentalUpdatedEmail(subscriber!, rental, selectedCopies);
            SendRentalUpdatedWhatsAppMessage(subscriber!, rental, selectedCopies);

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }

        public IActionResult Return(int id)
        {
            var rental = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(rc => rc.BookCopy)
                .ThenInclude(bc => bc!.Book)
                .FirstOrDefault(r => r.Id == id);

            // check if the rental is not found or not created today (to avoid editing rentals created in the past)
            if (rental is null || rental.CreatedOn.Date == DateTime.Today)
                return NotFound();

            var subscriber = _context.Subscribers
                .Include(s => s.Subscriptions)
                .FirstOrDefault(s => s.Id == rental.SubscriberId);

            var viewModel = new ReturnFormViewModel
            {
                Id = id,
                Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies.Where(c => !c.ReturnDate.HasValue).ToList()),
                SelectedCopies = rental.RentalCopies.Where(c => !c.ReturnDate.HasValue).Select(c => new ReturnCopyViewModel { Id = c.BookCopyId, IsReturned = c.ExtendedOn.HasValue ? false : null }).ToList(),
                AllowExtend = !subscriber!.IsBlackListed
                    && subscriber!.Subscriptions.Last().EndDate >= rental.StartDate.AddDays((int)RentalsConfigurations.MaxRentalDuration)
                    && rental.StartDate.AddDays((int)RentalsConfigurations.RentalDuration) >= DateTime.Today
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Return(ReturnFormViewModel model)
        {
            var rental = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(rc => rc.BookCopy)
                .ThenInclude(bc => bc!.Book)
                .FirstOrDefault(r => r.Id == model.Id);

            // check if the rental is not found or created today (in this case, he can edit or cancel the rental)
            if (rental is null || rental.CreatedOn.Date == DateTime.Today)
                return NotFound();

            var copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies).Where(c => !c.ReturnDate.HasValue).ToList();

            if (!ModelState.IsValid)
            {
                model.Copies = copies;
                return View(model);
            }

            var subscriber = _context.Subscribers
                .Include(s => s.Subscriptions)
                .FirstOrDefault(s => s.Id == rental.SubscriberId);

            if (model.SelectedCopies.Any(c => c.IsReturned.HasValue && !c.IsReturned.Value)) // check if the user wants to extend the rental (isReturned = false)
            {
                string error = string.Empty;

                if (subscriber!.IsBlackListed)
                    error = Errors.RentalNotAllowedForBlacklisted;

                else if (subscriber!.Subscriptions.Last().EndDate < rental.StartDate.AddDays((int)RentalsConfigurations.MaxRentalDuration))
                    error = Errors.RentalNotAllowedForInactive; // rental can't be extended for this subscriber before renwal

                else if (rental.StartDate.AddDays((int)RentalsConfigurations.RentalDuration) < DateTime.Today)
                    error = Errors.ExtendNotAllowed; // because the rental is expired and the user should return the books

                if (!string.IsNullOrEmpty(error))
                {
                    model.Copies = copies;
                    ModelState.AddModelError("", error);
                    return View(model);
                }
            }

            var isUpdated = false; // a flag to check if the rental is updated or not

            foreach (var copy in model.SelectedCopies)
            {
                if (!copy.IsReturned.HasValue) 
                    continue; // skip the copy if the user didn't select any action (return or extend)

                var currentCopyInDb = rental.RentalCopies.FirstOrDefault(c => c.BookCopyId == copy.Id); // get the current copy from the rental copies to update it

                if (currentCopyInDb is null) 
                    continue; 
                 
                if (copy.IsReturned.HasValue && copy.IsReturned.Value) // check if the user wants to return the copy (isReturned = true)
                {
                    if (currentCopyInDb.ReturnDate.HasValue)
                        continue;

                    currentCopyInDb.ReturnDate = DateTime.Now;
                    isUpdated = true;
                }

                if (copy.IsReturned.HasValue && !copy.IsReturned.Value) // check if the user wants to extend the rental (isReturned = false)
                {
                    if (currentCopyInDb.ExtendedOn.HasValue) 
                        continue;

                    currentCopyInDb.ExtendedOn = DateTime.Now;
                    currentCopyInDb.EndDate = currentCopyInDb.RentalDate.AddDays((int)RentalsConfigurations.MaxRentalDuration);
                    isUpdated = true;
                }
            }

            if (isUpdated)
            {
                rental.LastUpdatedOn = DateTime.Now;
                rental.LastUpdatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
                rental.PenaltyPaid = model.PenaltyPaid;

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Details), new { id = rental.Id });
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
            var rental = _context.Rentals
                .Include(r => r.RentalCopies)
                .Include(r => r.Subscriber)
                .FirstOrDefault(r => r.Id == id);

            if (rental is null)
                return NotFound("Rental not found.");

            if (rental.CreatedOn.Date != DateTime.Today)
                return BadRequest("You can't delete a rental that is not created today.");

            rental.IsDeleted = true;

            rental.LastUpdatedOn = DateTime.Now;
            rental.LastUpdatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // delete the rental copies
            // uncomment the below line if you want to delete the rental copies when the rental is deleted
            // _context.RentalCopies.RemoveRange(rental.RentalCopies);

            _context.SaveChanges();

            SendRentalCanceledEmail(rental.Subscriber!, rental);
            SendRentalCanceledWhatsAppMessage(rental.Subscriber!, rental);

            return Ok();
        }

        // a helper method to validate the subscriber before creating a rental
        private (string errorMessage, int? maxAllowedCopies) ValidateSubscriber(Subscriber subscriber, int? rentalId = null)
        {
            // check if the subscriber is blacklisted
            if (subscriber.IsBlackListed)
                return (errorMessage: Errors.BlackListedSubscriber, maxAllowedCopies: null);

            // check if the subscriber is inactive (i.e. the last subscription is ended)
            var lastSubscription = subscriber.Subscriptions.LastOrDefault();
            if (lastSubscription is null || lastSubscription.EndDate < DateTime.Today.AddDays((int)RentalsConfigurations.RentalDuration))
                return (errorMessage: Errors.InactiveSubscriber, maxAllowedCopies: null);

            // check if the subscriber has reached the max number of rentals
            // calculate the current rentals count for the subscriber
            var currentRentalsCount = subscriber.Rentals
                .Where(r => rentalId == null || r.Id != rentalId) // exclude the current rental from the count if it is an edit operation (rentalId is not null)
                .SelectMany(r => r.RentalCopies)
                .Count(rc => !rc.ReturnDate.HasValue);
            // calculate the available copies count that the subscriber can rent
            var availableCopiesCount = (int)RentalsConfigurations.MaxAllowedCopies - currentRentalsCount;
            if (availableCopiesCount.Equals(0))
                return (errorMessage: Errors.MaxCopiesReached, maxAllowedCopies: null);

            // if we reach here, then the subscriber is valid and can create a rental with the available copies count
            return (errorMessage: string.Empty, maxAllowedCopies: availableCopiesCount);
        }

        private (string errorMessage, ICollection<RentalCopy>? copies) ValidateCopies(List<BookCopy> selectedCopies, int subscriberId, int? rentalId = null)
        {
            if (selectedCopies is null || !selectedCopies.Any())
                return (errorMessage: "Copies selected are not found.", null);

            var currentSubscriberRentals = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .Where(r => r.SubscriberId == subscriberId && (rentalId == null || r.Id != rentalId))
                .SelectMany(r => r.RentalCopies)
                .Where(c => !c.ReturnDate.HasValue)
                .Select(c => c.BookCopy!.BookId)
                .ToList();

            List<RentalCopy> copies = new();

            foreach (var bookCopy in selectedCopies)
            {
                if (!bookCopy.IsAvailableForRental || !bookCopy.Book!.IsAvailableForRental)
                    return (errorMessage: Errors.NotAvilableRental, copies);

                if (bookCopy.Rentals.Any(c => !c.ReturnDate.HasValue && (rentalId == null || c.RentalId != rentalId)))
                    return (errorMessage: Errors.CopyIsInRental, copies);

                if (currentSubscriberRentals.Any(bookId => bookId == bookCopy.BookId))
                    return (errorMessage: $"This subscriber has already rented a copy of '{bookCopy.Book!.Title}' book", copies);

                copies.Add(new RentalCopy { BookCopyId = bookCopy.Id });
            }

            return (errorMessage: string.Empty, copies);
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

        // a helper method to send an email to the subscriber after creating a rental
        private void SendRentalConfirmationEmail(Subscriber subscriber, Rental rental, List<BookCopy> selectedCopies)
        {
            var placeholders = new Dictionary<string, string>
            {
                { "mediaUrl", "https://res.cloudinary.com/trojan74/image/upload/v1740774488/icon-positive-vote-1_qrtznr.svg" },
                { "header", $"Hey {subscriber.FirstName}," },
                { "body", $"Thanks for renting from LitraLand! We're excited to have you 🤩.<br><br>" +
                          $"Your rental has been successfully created with the following details:<br>" +
                          $"🆔 <strong>Rental ID:</strong> {rental.Id}<br>" +
                          $"📅 <strong>Rental Date:</strong> {rental.StartDate.ToString("dd MMM, yyyy")}<br>" +
                          $"⏳ <strong>Rental Duration:</strong> {(int)RentalsConfigurations.RentalDuration} days<br>" +
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
        private void SendRentalConfirmationWhatsAppMessage(Subscriber subscriber, Rental rental, List<BookCopy> selectedCopies)
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
                        new WhatsAppTextParameter { Text = rental.Id.ToString() },
                        new WhatsAppTextParameter { Text = rental.StartDate.ToString("dd MMM, yyyy")},
                        new WhatsAppTextParameter { Text = ((int)RentalsConfigurations.RentalDuration).ToString() },
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

        private void SendRentalUpdatedEmail(Subscriber subscriber, Rental rental, List<BookCopy> selectedCopies)
        {
            var placeholders = new Dictionary<string, string>
            {
                { "mediaUrl", "https://res.cloudinary.com/trojan74/image/upload/v1741402646/calendar_zfohjc_vdrflq.png" },
                { "header", $"Hey {subscriber.FirstName}," },
                { "body", $"Your rental details have been updated successfully! ✨<br><br>" +
                          $"Here are the updated details:<br>" +
                          $"🆔 <strong>Rental ID:</strong> {rental.Id}<br>" +
                          $"📅 <strong>Rental Date:</strong> {rental.StartDate:dd MMM, yyyy}<br>" +
                          $"⏳ <strong>Rental Duration:</strong> {(int)RentalsConfigurations.RentalDuration} days<br>" +
                          $"📚 <strong>Rented Books:</strong> {string.Join(", ", selectedCopies.Select(c => c.Book!.Title))}<br><br>" +
                          $"Please check your updated rental details. If you have any questions, feel free to contact us! 😊<br>" +
                          $"Enjoy reading! 📖😊"}
            };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
                subscriber.Email,
                "Rental Updated",
                body
            ));
        }

        private void SendRentalUpdatedWhatsAppMessage(Subscriber subscriber, Rental rental, List<BookCopy> selectedCopies)
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
                        new WhatsAppTextParameter { Text = rental.Id.ToString() },
                        new WhatsAppTextParameter { Text = rental.StartDate.ToString("dd MMM, yyyy") },
                        new WhatsAppTextParameter { Text = ((int)RentalsConfigurations.RentalDuration).ToString() },
                        new WhatsAppTextParameter { Text = string.Join(", ", selectedCopies.Select(c => c.Book!.Title))}
                    }
                }
            };

            var phoneNumber = _webHostEnvironment.IsDevelopment() ? "01129816608" : subscriber.PhoneNumber;

            BackgroundJob.Enqueue(() => _whatsAppClient.SendMessage(
                $"2{phoneNumber}",
                WhatsAppLanguageCode.English,
                WhatsAppTemplates.RentalUpdated,
                components
            ));
        }

        private void SendRentalCanceledEmail(Subscriber subscriber, Rental rental)
        {
            var placeholders = new Dictionary<string, string>
            {
                { "mediaUrl", "https://res.cloudinary.com/trojan74/image/upload/v1741402646/calendar_zfohjc_vdrflq.png" },
                { "header", $"Hey {subscriber.FirstName}," },
                { "body", $"We regret to inform you that your rental has been canceled successfully.<br><br>" +
                          $"🆔 <strong>Rental ID:</strong> {rental.Id}<br>" +
                          $"If this was a mistake or you need assistance, please contact us.<br>" +
                          $"We hope to serve you again soon! 📖😊" }
            };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
                subscriber.Email,
                "Rental Canceled",
                body
            ));
        }

        private void SendRentalCanceledWhatsAppMessage(Subscriber subscriber, Rental rental)
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
                        new WhatsAppTextParameter { Text = rental.Id.ToString() },
                        new WhatsAppTextParameter { Text = rental.StartDate.ToString("dd MMM, yyyy") }
                    }
                }
            };

            var phoneNumber = _webHostEnvironment.IsDevelopment() ? "01129816608" : subscriber.PhoneNumber;

            BackgroundJob.Enqueue(() => _whatsAppClient.SendMessage(
                $"2{phoneNumber}",
                WhatsAppLanguageCode.English,
                WhatsAppTemplates.RentalCanceled,
                components
            ));
        }

    }
}