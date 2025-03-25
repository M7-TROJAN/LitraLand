namespace LitraLand.Web.Core.Models
{
    public class Area
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public int GovernorateId { get; set; }
        public Governorate Governorate { get; set; } = null!;
        public ICollection<Subscriber> Subscribers { get; set; } = new List<Subscriber>();
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
