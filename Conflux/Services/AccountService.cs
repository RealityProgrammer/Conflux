using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Conflux.Services;

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

    public async Task<bool> IsProfileSetup() {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var user = await userManager.GetUserAsync(authState.User);

        return user?.IsProfileSetup ?? false;
    }

    public async Task<bool> IsProfileSetup(ClaimsPrincipal claimsPrincipal) {
        return await userManager.GetUserAsync(claimsPrincipal) is { IsProfileSetup: true };
    }

    public async Task MarkProfileSetup() {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        await MarkProfileSetup(authState.User);
    }

    public async Task MarkProfileSetup(ClaimsPrincipal claimsPrincipal) {
        var user = await userManager.GetUserAsync(claimsPrincipal);

        if (user == null) return;

        user.IsProfileSetup = true;
        await userManager.UpdateAsync(user);
    }
}