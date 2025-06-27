using LitraLand.Domain.Entities.Common;

namespace LitraLand.Domain.Entities.Community
{
    public class CommunityBook
    {
        public int Id { get; set; }

        // Basic Info
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string Description { get; set; } = null!;

        // Images
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string? ImagePublicId { get; set; } // cloudinary public id

        //
        public decimal Price { get; set; }
        public bool IsForExchange { get; set; }

        // Ownership
        public string OwnerId { get; set; } = null!;
        public ApplicationUser Owner { get; set; } = null!;

        // Metadata
        public DateTime CreatedOn { get; set; }
    }
}