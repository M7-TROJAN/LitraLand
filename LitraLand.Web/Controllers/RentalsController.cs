using Microsoft.AspNetCore.DataProtection;

namespace LitraLand.Web.Controllers
{
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Reception)]
    public class RentalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataProtector _dataProtector;
        //private readonly IImageServices _imageServices;
        //private readonly ICloudinaryService _cloudinaryService;
        //private readonly IWhatsAppClient _whatsAppClient;
        //private readonly IWebHostEnvironment _webHostEnvironment;
        //private readonly IEmailSender _emailSender;
        //private readonly IEmailBodyBuilder _emailBodyBuilder;

        public RentalsController(ApplicationDbContext context, IMapper mapper, IDataProtectionProvider dataProtector)
        {
            _context = context;
            _mapper = mapper;
        }
        public IActionResult Index()
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

        public IActionResult Create(string sKey)
        {
            var viewModel = new RentalFormViewModel
            {
                SubscriberKey = sKey
            };

            return View(viewModel);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Create(RentalFormViewModel model)
        //{
        //}

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