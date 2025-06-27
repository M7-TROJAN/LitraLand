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

        /// <summary>
        /// Get the user's application area.
        /// </summary>
        public static string? GetUserApplicationArea(this ClaimsPrincipal user) =>
            user.FindFirstValue(CustomClaimTypes.UserAppllicationArea);

        /// <summary>
        /// check if the user is a library staff member.
        /// </summary>
        public static bool IsLibraryStaff(this ClaimsPrincipal user) =>
            user.IsInRole(AppRoles.SuperAdmin) || user.IsInRole(AppRoles.LibraryAdmin)
                || user.IsInRole(AppRoles.Archive) || user.IsInRole(AppRoles.Reception);

        /// <summary>
        /// check if the user is an admin (any type of admin).
        /// </summary>
        public static bool IsAdmin(this ClaimsPrincipal user) =>
            user.IsInRole(AppRoles.SuperAdmin) || user.IsInRole(AppRoles.CommunityAdmin)
                || user.IsInRole(AppRoles.LibraryAdmin);

        /// <summary>
        /// check if the user is a super admin.
        /// </summary>
        public static bool IsSuperAdmin(this ClaimsPrincipal user) =>
            user.IsInRole(AppRoles.SuperAdmin);

        /// <summary>
        /// check if the user is a library admin.
        /// </summary>
        public static bool IsLibraryAdmin(this ClaimsPrincipal user) =>
            user.IsInRole(AppRoles.LibraryAdmin);

        /// <summary>
        /// check if the user is a community admin.
        /// </summary>
        public static bool IsCommunityAdmin(this ClaimsPrincipal user) =>
            user.IsInRole(AppRoles.CommunityAdmin);

        /// <summary>
        /// check if the user is a community member.
        /// </summary>
        public static bool IsCommunityMember(this ClaimsPrincipal user) =>
            user.IsInRole(AppRoles.CommunityMember);

        /// <summary>
        /// check if the user is an archive staff member.
        /// </summary>
        public static bool IsArchive(this ClaimsPrincipal user) =>
            user.IsInRole(AppRoles.Archive);

        /// <summary>
        /// check if the user is a reception staff member.
        /// </summary>
        public static bool IsReception(this ClaimsPrincipal user) =>
            user.IsInRole(AppRoles.Reception);

        /// <summary>
        /// check if the user is authenticated.
        /// </summary>
        public static bool IsAuthenticated(this ClaimsPrincipal user) =>
            user.Identity != null && user.Identity.IsAuthenticated;
    }
}
