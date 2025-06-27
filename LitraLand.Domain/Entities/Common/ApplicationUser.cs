using LitraLand.Domain.Entities.Community;
using Microsoft.AspNetCore.Identity;

namespace LitraLand.Domain.Entities.Common
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string? ImagePublicId { get; set; } // cloudinary public id
        public int? AreaId { get; set; }
        public Area? Area { get; set; } // navigation property
        public int? GovernorateId { get; set; }
        public Governorate? Governorate { get; set; } // navigation property
        public string? Address { get; set; }
        public bool IsDeleted { get; set; }
        public string? CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string? LastUpdatedById { get; set; }
        public string? AppllicationArea { get; set; }
        public ICollection<CommunityBook> CommunityBooks { get; set; } = new List<CommunityBook>(); // books owned by the user (community members)
    }
}