namespace LitraLand.Domain.Entities.Library
{
    public class Author : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<Book>? Books { get; set; }
    }
}
