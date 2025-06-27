namespace LitraLand.Web.Areas.Community.Core.ViewModels.CommunityBookViews
{
    public class BookSearchRequest
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? Search { get; set; }
        public string? Filter { get; set; } // "Sell", "Exchange", or null
    }
}
