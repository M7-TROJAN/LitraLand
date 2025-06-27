namespace LitraLand.Web.Areas.Community.Core.ViewModels.CommunityMemberViews
{
    public class TopMemberViewModel
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public int NumberOfBooks { get; set; }
        public string? ImageUrl { get; set; }
    }
}
