namespace LitraLand.Domain.Dtos.Library
{
    public class MostPopularBookDTO
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageThumbnailUrl { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public int RentalCount { get; set; }
    }
}
