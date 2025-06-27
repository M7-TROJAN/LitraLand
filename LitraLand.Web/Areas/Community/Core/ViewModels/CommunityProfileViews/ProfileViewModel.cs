namespace LitraLand.Web.Areas.Community.Core.ViewModels.CommunityProfileViews
{
    public class ProfileViewModel
    {
        public MemberViewModel? Member { get; set; }
        public bool HasBooks { get; set; }
        public int TotalBooks { get; set; }
    }
}