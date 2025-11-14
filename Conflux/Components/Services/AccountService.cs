using Conflux.Components.Services.Abstracts;
using Conflux.Database;
using Conflux.Database.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Conflux.Components.Services;

public class AccountService(UserManager<ApplicationUser> userManager, AuthenticationStateProvider authStateProvider) : IAccountService {
    public async Task<ApplicationUser?> GetCurrentUserAsync() {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var user = await userManager.GetUserAsync(authState.User);

        return user;
    }

    public async Task<bool> IsTwoFactorEnabled() {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        
        return await IsTwoFactorEnabled(authState.User);
    }

    public async Task<bool> IsTwoFactorEnabled(ClaimsPrincipal claimsPrincipal) {
        var user = await userManager.GetUserAsync(claimsPrincipal);
        
        if (user == null) return false;
        
        return await userManager.GetTwoFactorEnabledAsync(user);
    }

    public Task<bool> IsTwoFactorEnabled(ApplicationUser user) {
        return userManager.GetTwoFactorEnabledAsync(user);
    }
}