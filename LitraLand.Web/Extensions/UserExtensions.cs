namespace LitraLand.Web.Extensions
{
    public static class UserExtensions
    {
        /// <summary>
        /// Get the user ID from claims.
        /// </summary>
        public static string GetUserId(this ClaimsPrincipal user) =>
            user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // <summary>
        /// Get the userName from claims.
        /// </summary>
        public static string GetUserName(this ClaimsPrincipal user) =>
            user.FindFirstValue(ClaimTypes.Name)!;

        /// <summary>
        /// Get the full name
        /// </summary>
        public static string? GetFullName(this ClaimsPrincipal user) =>
            user.FindFirstValue(ClaimTypes.GivenName);

        /// <summary>
        /// Get the user's email.
        /// </summary>
        public static string GetEmail(this ClaimsPrincipal user) =>
            user.FindFirstValue(ClaimTypes.Email)!;

        /// <summary>
        /// Get the user's Image thumbnail url.
        /// </summary>
        public static string? GetImageThumbnail(this ClaimsPrincipal user) =>
            user.FindFirstValue(CustomClaimTypes.ImageThumbnailUrl);
    }
}
