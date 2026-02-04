using Conflux.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Conflux.Web.Authentication;

public sealed class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole<Guid>> {
    public ApplicationUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole<Guid>> roleManager,
        IOptions<IdentityOptions> options
    ) : base(userManager, roleManager, options) {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user) {
        var identity = await base.GenerateClaimsAsync(user);
    
        identity.AddClaim(new("ProfileSetup", user.IsProfileSetup.ToString()));
        
        return identity;
    }
}