using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Conflux.Services;

public sealed class CurrentUserService(
    IUserService userService, 
    AuthenticationStateProvider authStateProvider, 
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager
) : ICurrentUserService {
    
    private ApplicationUser? _currentUser;

    public async Task<ApplicationUser?> GetCurrentUserAsync() {
        if (_currentUser != null) {
            return _currentUser;
        }
        
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity is { IsAuthenticated: true }) {
            _currentUser = await userService.GetUserAsync(user);
        }

        return _currentUser;
    }

    public async Task RefreshUserAsync() {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity is { IsAuthenticated: true }) {
            _currentUser = await userService.GetUserAsync(user);
        }
    }

    public Task UpdateUserAsync() {
        if (_currentUser == null) return Task.CompletedTask;
        
        return userManager.UpdateAsync(_currentUser);
    }

    public Task LogoutAsync() {
        _currentUser = null;
        return signInManager.SignOutAsync();
    }
}