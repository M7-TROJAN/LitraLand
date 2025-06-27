namespace LitraLand.Web.Areas.Community.Core.ViewModels.CommunityBookViews
{
    public class CommunityBookViewModel
    {
        // Unique Identifier
        public string Key { get; set; } = null!;

        // Basic Info
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string Description { get; set; } = null!;

        // Images
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }

        //
        public decimal Price { get; set; }
        public bool IsForExchange { get; set; }

        // Metadata
        public DateTime CreatedOn { get; set; }

        // Owner Information
        public string OwnerId { get; set; } = null!;
        public string OwnerUserName { get; set; } = null!;
        public string OwnerEmail { get; set; } = null!;
        public string? OwnerPhoneNumber { get; set; }
    }
}