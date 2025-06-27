namespace LitraLand.Domain.Consts
{
    public static class AppConstants
    {
        public const string AllowedImageExtensions = ".jpg, .jpeg, .png, .webp";
        public const long MaxImageSize = 2 * 1024 * 1024; // 2097152 bytes (2MB)
        public const string NoBookImage = "/images/books/no-book.jpg";
        public const string ImagePlaceholder = "/images/image_placeholder.jpg";
        public const string DefaultAvatarUrl = "/assets/images/avatar2.webp";
        public const string LibraryStaffArea = "Library";
        public const string CommunityArea = "Community";
    }
}