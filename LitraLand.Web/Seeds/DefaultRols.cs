using Microsoft.AspNetCore.Identity;

namespace LitraLand.Web.Seeds
{
    public static class DefaultRols
    {
        public static async Task SeedRolsAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.Roles.AnyAsync())
            {
                await roleManager.CreateAsync(new IdentityRole(AppRoles.Admin));
                await roleManager.CreateAsync(new IdentityRole(AppRoles.SuperAdmin));
                await roleManager.CreateAsync(new IdentityRole(AppRoles.Archive));
                await roleManager.CreateAsync(new IdentityRole(AppRoles.Reception));
                await roleManager.CreateAsync(new IdentityRole(AppRoles.User));
            }
        }
    }
}
