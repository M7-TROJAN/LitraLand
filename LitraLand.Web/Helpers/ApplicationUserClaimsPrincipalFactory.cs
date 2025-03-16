using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace LitraLand.Web.Helpers
{
    public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public ApplicationUserClaimsPrincipalFactory
            (UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> optionsAccessor) : base(userManager, roleManager, optionsAccessor)
        {
        }
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FullName));
            identity.AddClaim(new Claim(CustomClaimTypes.ImageThumbnailUrl, user.ImageThumbnailUrl ?? AppConstants.DefaultAvatarUrl));
            // Add more custom claims here if needed
            return identity;
        }
    }
}