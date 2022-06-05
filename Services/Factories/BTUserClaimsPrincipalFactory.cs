using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TheBugTracker.Models;

namespace TheBugTracker.Services.Factories
{
    public class BTUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<BTUser, IdentityRole>
    {
        public BTUserClaimsPrincipalFactory(
            UserManager<BTUser> userManager, 
            RoleManager<IdentityRole> roleManager, 
            IOptions<IdentityOptions> options
        ) : base(userManager, roleManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BTUser user)
        {
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()));
            return identity;            
        }
    }
}