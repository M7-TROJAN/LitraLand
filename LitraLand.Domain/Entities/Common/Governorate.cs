using LitraLand.Domain.Entities.Library;

namespace LitraLand.Domain.Entities.Common
{
    public class Governorate
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public ICollection<Area> Areas { get; set; } = new List<Area>();
        public ICollection<Subscriber> Subscribers { get; set; } = new List<Subscriber>();
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        public override string ToString() => $"{Name}";
    }
}