using Conflux.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Conflux.Services;

public sealed class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser> {
    public ApplicationUserClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, IOptions<IdentityOptions> options) : base(userManager, options) {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user) {
        var identity = await base.GenerateClaimsAsync(user);
    
        identity.AddClaim(new("ProfileSetup", user.IsProfileSetup.ToString()));
        
        return identity;
    }
}