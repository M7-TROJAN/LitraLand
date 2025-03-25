namespace LitraLand.Web.Core.Models
{
    public class BookCopy : BaseModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; } // Navigation property (relation is one-to-many)
        public bool IsAvailableForRental { get; set; }
        public int EditionNumber { get; set; }
        public int SerialNumber { get; set; }
        public ICollection<RentalCopy> Rentals { get; set; } = new List<RentalCopy>();
    }
}
