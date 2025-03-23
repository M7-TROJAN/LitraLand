namespace LitraLand.Web.Core.Consts.DTO
{
    public class MostPopularBookDTO
    {
        public int BookId { get; set; }
        public string? Title { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string? AuthorName { get; set; }
        public int RentalCount { get; set; }
    }
}
