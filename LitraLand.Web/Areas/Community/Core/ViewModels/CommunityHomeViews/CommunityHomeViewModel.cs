namespace LitraLand.Web.Areas.Community.Core.ViewModels.CommunityHomeViews
{
    public class CommunityHomeViewModel
    {
        public int NumberOfBooks { get; set; }
        public int NumberOfMembers { get; set; }
        public List<TopMemberViewModel> TopMembers { get; set; } = new List<TopMemberViewModel>();
    }
}