namespace LitraLand.Web.Core.ViewModels.SubscriberViews
{
    public class SubscriberFormViewModel
    {
        public string? Key { get; set; }

        [Display(Name = "First name")]
        [MaxLength(100, ErrorMessage = Errors.MaxLength)]
        [RegularExpression(RegexPatterns.DenySpecialCharacters, ErrorMessage = Errors.DenySpecialCharacters)]
        public string FirstName { get; set; } = null!;

        [Display(Name = "Last name")]
        [MaxLength(100, ErrorMessage = Errors.MaxLength)]
        [RegularExpression(RegexPatterns.DenySpecialCharacters, ErrorMessage = Errors.DenySpecialCharacters)]
        public string LastName { get; set; } = null!;

        [Display(Name = "Date of Birth")]
        [AssertThat("DateOfBirth <= Today()", ErrorMessage = Errors.NotAllowFutureDates)]
        public DateTime DateOfBirth { get; set; } = DateTime.Now;

        [Display(Name = "National ID")]
        [MaxLength(14, ErrorMessage = Errors.MaxLength)]
        [RegularExpression(RegexPatterns.NationalId, ErrorMessage = Errors.InvalidNationalId)]
        [Remote("AllowNationalId", null /*"Subscribers"*/, AdditionalFields = "Key", ErrorMessage = Errors.Duplicated)]
        public string NationalId { get; set; } = null!;

        [Display(Name = "Phone Number")]
        [MaxLength(11, ErrorMessage = Errors.MaxLength)]
        [RegularExpression(RegexPatterns.MobileNumber, ErrorMessage = Errors.InvalidMobileNumber)]
        [Remote("AllowPhoneNumber", null /*"Subscribers"*/, AdditionalFields = "Key", ErrorMessage = Errors.Duplicated)]
        public string PhoneNumber { get; set; } = null!;

        [Display(Name = "Has WhatsApp?")]
        public bool HasWhatsApp { get; set; }

        [EmailAddress]
        [MaxLength(100, ErrorMessage = Errors.MaxLength)]
        [Remote("AllowEmail", null /*"Subscribers"*/, AdditionalFields = "Key", ErrorMessage = Errors.Duplicated)]
        public string Email { get; set; } = null!;

        [RequiredIf("Key == null || Key == ''", ErrorMessage = Errors.EmptyImage)]
        public IFormFile? Image { get; set; }

        [Display(Name = "Area")]
        [Required(ErrorMessage = Errors.Required)]
        public int AreaId { get; set; }
        public IEnumerable<SelectListItem>? Areas { get; set; } = new List<SelectListItem>();

        [Display(Name = "Governorate")]
        [Required(ErrorMessage = Errors.Required)]
        public int GovernorateId { get; set; }
        public IEnumerable<SelectListItem>? Governorates { get; set; }

        [MaxLength(500, ErrorMessage = Errors.MaxLength)]
        public string Address { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string? ImagePublicId { get; set; } // cloudinary public id
    }
}
