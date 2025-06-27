namespace LitraLand.Web.Areas.Community.Core.ViewModels.CommunityMemberViews
{
    public class MemberViewModel
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public DateTime MembershipDate { get; set; }
        public string? Address { get; set; }
        public string? Area { get; set; }
        public string? Governorate { get; set; }
    }
}
