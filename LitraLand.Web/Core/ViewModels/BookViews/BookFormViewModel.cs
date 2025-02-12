namespace LitraLand.Web.Core.ViewModels.BookViews
{
    public class BookFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(500, ErrorMessage = Errors.MaxLength)]
        [Remote("AllowItem", null, AdditionalFields = "Id,AuthorId", ErrorMessage = Errors.DuplicatedBook)] // ignore the duplicate title with the same author
        public string Title { get; set; } = null!;

        [Display(Name = "Author")]
        [Remote("AllowItem", null, AdditionalFields = "Id,Title", ErrorMessage = Errors.DuplicatedBook)] // ignore the duplicate title with the same author
        public int AuthorId { get; set; }

        public IEnumerable<SelectListItem>? Authors { get; set; }

        [MaxLength(200, ErrorMessage = Errors.MaxLength)]
        public string Publisher { get; set; } = null!;

        [Display(Name = "Published Date")]
        [AssertThat("PublishedDate <= Today()", ErrorMessage = Errors.NotAllowFutureDates)]
        public DateTime PublishedDate { get; set; } = DateTime.Now;

        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string? ImagePublicId { get; set; }

        [MaxLength(50, ErrorMessage = Errors.MaxLength)]
        public string Hall { get; set; } = null!;

        [Display(Name = "Is Book Available for Rental?")]
        public bool IsAvailableForRental { get; set; }

        public string Description { get; set; } = null!;

        [Display(Name = "Categories")]
        [Required(ErrorMessage = Errors.Required)]
        public IList<int> SelectedCategories { get; set; } = new List<int>();

        public IEnumerable<SelectListItem>? Categories { get; set; }
    }
}




