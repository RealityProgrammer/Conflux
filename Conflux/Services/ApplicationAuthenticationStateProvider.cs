using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Conflux.Services;

internal sealed class ApplicationAuthenticationStateProvider(
    ILoggerFactory loggerFactory,
    IServiceScopeFactory scopeFactory,
    IOptions<IdentityOptions> options
) : RevalidatingServerAuthenticationStateProvider(loggerFactory) {
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken
    ) {
        await using var scope = scopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        ClaimsPrincipal claimsPrinciple = authenticationState.User;

        var applicationUser = await userManager.GetUserAsync(claimsPrinciple);

        if (applicationUser is null) {
            return false;
        }

        if (userManager.SupportsUserSecurityStamp) {
            var principalStamp = claimsPrinciple.FindFirstValue(options.Value.ClaimsIdentity.SecurityStampClaimType);
            var securityStamp = await userManager.GetSecurityStampAsync(applicationUser);

            if (principalStamp != securityStamp) {
                return false;
            }
        }

        return true;

        // var profileSetupClaim = claimsPrinciple.FindFirst("ProfileSetup");
        //
        // if (profileSetupClaim == null || profileSetupClaim.Value != applicationUser.IsProfileSetup.ToString()) {
        //     return false;
        // }
        //
        // return true;
    }

    // public override async Task<AuthenticationState> GetAuthenticationStateAsync() {
    //     var authState = await base.GetAuthenticationStateAsync();
    //     var claimsPrinciple = authState.User;
    //
    //     if (claimsPrinciple.Identity?.IsAuthenticated ?? false) {
    //         await using var scope = scopeFactory.CreateAsyncScope();
    //         var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    //         
    //         var applicationUser = await userManager.GetUserAsync(claimsPrinciple);
    //
    //         if (applicationUser is null) {
    //             return authState;
    //         }
    //         
    //         var isProfileSetup = applicationUser.IsProfileSetup;
    //
    //         // Add profile setup claim if not present or changed
    //         var profileSetupClaim = claimsPrinciple.FindFirst("ProfileSetup");
    //
    //         if (profileSetupClaim == null || profileSetupClaim.Value != isProfileSetup.ToString()) {
    //             var identity = claimsPrinciple.Identity as ClaimsIdentity;
    //             
    //             // Remove existing claim if present
    //             var existingClaim = identity!.FindFirst("ProfileSetup");
    //
    //             if (existingClaim != null) {
    //                 identity.RemoveClaim(existingClaim);
    //             }
    //
    //             // Add new claim
    //             identity.AddClaim(new("ProfileSetup", isProfileSetup.ToString()));
    //             
    //             // Return new authentication state with modified principal
    //             return new(claimsPrinciple);
    //             
    //             // var claims = new List<Claim>(claimsPrinciple.Claims);
    //             //
    //             // // Replacing the claim
    //             // claims.RemoveAll(c => c.Type == "ProfileSetup");
    //             // claims.Add(new("ProfileSetup", isProfileSetup.ToString()));
    //             //
    //             // var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
    //             // return new(new(identity));
    //         }
    //     }
    //
    //     return authState;
    // }
}