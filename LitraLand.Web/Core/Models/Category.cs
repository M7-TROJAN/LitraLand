namespace LitraLand.Web.Core.Models
{
    public class Category : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // null forgiving operator thats means it will never be null
        public ICollection<BookCategory> Books { get; set; } = new List<BookCategory>(); // category has many-to-many relationship with book
    }
}
