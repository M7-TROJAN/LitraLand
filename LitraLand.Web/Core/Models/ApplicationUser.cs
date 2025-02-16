using Microsoft.AspNetCore.Identity;

namespace LitraLand.Web.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public string? CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string? LastUpdatedById { get; set; }
    }
}
