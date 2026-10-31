using System.Security.Claims;
using OnlineShoppingCenter.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace OnlineShoppingCenter.Data
{
    public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, Role>
    {
        public CustomClaimsPrincipalFactory(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IOptions<IdentityOptions> options)
            : base(userManager, roleManager, options) { }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
            return identity;
        }
    }
}
