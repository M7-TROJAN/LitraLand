namespace LitraLand.Domain.Consts
{
    public static class AppRoles
    {
        public const string SuperAdmin = "SuperAdmin";

        public const string LibraryAdmin = "LibraryAdmin";
        public const string Archive = "Archive";
        public const string Reception = "Reception";

        public const string CommunityAdmin = "CommunityAdmin";
        public const string CommunityMember = "CommunityMember";

        public static readonly List<string> LibraryStaffRoles = new List<string> { SuperAdmin, LibraryAdmin, Archive, Reception };

        public static readonly List<string> CommunityRoles = new List<string> { CommunityAdmin, CommunityMember };
    }
}