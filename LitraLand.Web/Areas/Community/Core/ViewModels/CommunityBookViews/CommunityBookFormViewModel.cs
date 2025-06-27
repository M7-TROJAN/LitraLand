namespace LitraLand.Web.Areas.Community.Core.ViewModels.CommunityBookViews
{
    public class CommunityBookFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(500, ErrorMessage = Errors.MaxLength)]
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string Description { get; set; } = null!;

        [RequiredIf("Id == 0", ErrorMessage = "The book image is required.")]
        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string? ImagePublicId { get; set; }
        public decimal Price { get; set; }

        [Display(Name = "Is Book Available for Exchange?")]
        public bool IsForExchange { get; set; }

    }
}