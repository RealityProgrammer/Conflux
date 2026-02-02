using Conflux.Domain.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Conflux.Web.Authentication;

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
    }
}