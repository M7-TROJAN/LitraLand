using Microsoft.AspNetCore.Identity;

namespace LitraLand.Web.Seeds
{
    public static class DefaultUsers
    {
        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            ApplicationUser admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@litraland.com",
                FullName = "Admin",
                EmailConfirmed = true

            };

            // first check if the admin user already exists
            var user = await userManager.FindByEmailAsync(admin.Email);

            // if the admin user does not exist, create it
            if (user is null)
            {
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, AppRoles.Admin);
            }
        }
    }
}
