namespace LitraLand.Web.Core.Models
{
    public class BaseModel
    {
        public DateTime CreatedOn { get; set; }
        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; } // navigation property
        public DateTime? LastUpdatedOn { get; set; }
        public string? LastUpdatedById { get; set; }
        public ApplicationUser? LastUpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}
