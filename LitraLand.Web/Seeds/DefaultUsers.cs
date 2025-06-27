using LitraLand.Domain.Entities.Common;
using Microsoft.AspNetCore.Identity;

namespace LitraLand.Web.Seeds
{
    public static class DefaultUsers
    {
        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            // seed the super admin user
            ApplicationUser superAdmin = new ApplicationUser
            {
                UserName = "superadmin",
                Email = "superadmin@litraland.com",
                FullName = "Super Admin",
                AppllicationArea = AppConstants.LibraryStaffArea,
                EmailConfirmed = true
            };

            // first check if the super admin user already exists
            var user = await userManager.FindByEmailAsync(superAdmin.Email);

            // if the super admin user does not exist, create it
            if (user is null)
            {
                await userManager.CreateAsync(superAdmin, "superAdmin@123");
                await userManager.AddToRoleAsync(superAdmin, AppRoles.SuperAdmin);
            }

            // seed the libraryadmin user
            //ApplicationUser admin = new ApplicationUser
            //{
            //    UserName = "libraryadmin",
            //    Email = "libraryadmin@litraland.com",
            //    FullName = "Library Admin",
            //    AppllicationArea = AppConstants.LibraryStaffArea,
            //    EmailConfirmed = true

            //};

            //// first check if the libraryadmin user already exists
            //user = await userManager.FindByEmailAsync(admin.Email);

            //// if the libraryadmin user does not exist, create it
            //if (user is null)
            //{
            //    await userManager.CreateAsync(admin, "Admin@123");
            //    await userManager.AddToRoleAsync(admin, AppRoles.LibraryAdmin);
            //}

            //// seed the communityadmin user
            //ApplicationUser communityadmin = new ApplicationUser
            //{
            //    UserName = "communityadmin",
            //    Email = "community_admin@LitraLand.com",
            //    FullName = "Community Admin",
            //    AppllicationArea = AppConstants.CommunityArea,
            //    EmailConfirmed = true
            //};

            //// first check if the communityadmin user already exists
            //user = await userManager.FindByEmailAsync(communityadmin.Email);

            //// if the communityadmin user does not exist, create it
            //if (user is null)
            //{
            //    await userManager.CreateAsync(communityadmin, "Admin@123");
            //    await userManager.AddToRoleAsync(communityadmin, AppRoles.CommunityAdmin);
            //}
        }
    }
}