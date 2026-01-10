using Conflux.Domain;
using Conflux.Domain.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Conflux.Services;

public class UserService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IDbContextFactory<ApplicationDbContext> DbContextFactory
) : IUserService {
    public Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal claimsPrincipal) {
        return userManager.GetUserAsync(claimsPrincipal);
    }
    
    public async Task<bool> IsUserNameTaken(string username) {
        await using (var dbContext = await DbContextFactory.CreateDbContextAsync()) {
            return await dbContext.Users.AnyAsync(user => userManager.NormalizeName(username).Equals(user.NormalizedUserName, StringComparison.InvariantCultureIgnoreCase));
        }
    }

    public async Task<bool> IsTwoFactorEnabled(ClaimsPrincipal claimsPrincipal) {
        var user = await userManager.GetUserAsync(claimsPrincipal);
        
        if (user == null) return false;
        
        return await userManager.GetTwoFactorEnabledAsync(user);
    }

    public Task<bool> IsTwoFactorEnabled(ApplicationUser user) {
        return userManager.GetTwoFactorEnabledAsync(user);
    }

    public Task<bool> IsProfileSetup(ClaimsPrincipal claimsPrincipal) {
        return Task.FromResult(claimsPrincipal.FindFirstValue("ProfileSetup") == bool.TrueString);
    }

    public async Task UpdateProfileSetup(ClaimsPrincipal claimsPrincipal, bool value) {
        var user = await userManager.GetUserAsync(claimsPrincipal);

        if (user == null || user.IsProfileSetup == value) return;
        
        user.IsProfileSetup = value;
        
        IdentityResult result = await userManager.UpdateAsync(user);
        
        GC.KeepAlive(result);
        
        await signInManager.RefreshSignInAsync(user);
    }

    public Task<IdentityResult> AssignRoleAsync(ApplicationUser user, string roleName) {
        return userManager.AddToRoleAsync(user, roleName);
    }

    public Task<IdentityResult> RemoveRoleAsync(ApplicationUser user, string roleName) {
        return userManager.RemoveFromRoleAsync(user, roleName);
    }

    public Task<IList<string>> GetRolesAsync(ApplicationUser user) {
        return userManager.GetRolesAsync(user);
    }
}