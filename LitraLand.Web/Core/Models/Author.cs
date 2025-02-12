namespace LitraLand.Web.Core.Models
{
    public class Author : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<Book>? Books { get; set; }
    }
}
