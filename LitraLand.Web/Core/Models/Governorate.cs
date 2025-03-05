namespace LitraLand.Web.Core.Models
{
    public class Governorate : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<Area> Areas { get; set; } = new List<Area>();
        public ICollection<Subscriber> Subscribers { get; set; } = new List<Subscriber>();
    }
}