namespace LitraLand.Domain.Entities.Library
{
    public class BookCategory
    {
        public int BookId { get; set; }
        public Book? Book { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
