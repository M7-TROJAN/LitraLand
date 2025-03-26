namespace LitraLand.Domain.Entities
{
    public class Book : BaseEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int AuthorId { get; set; }
        public Author? Author { get; set; }
        public string Publisher { get; set; } = null!;
        public DateTime PublishedDate { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string? ImagePublicId { get; set; } // cloudinary public id
        public string Hall { get; set; } = null!;
        public bool IsAvailableForRental { get; set; }
        public string Description { get; set; } = null!;
        public ICollection<BookCategory> Categories { get; set; } = new List<BookCategory>(); // book has many-to-many relationship with category
        public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>(); // book has one-to-many relationship with book copy
    }
}
