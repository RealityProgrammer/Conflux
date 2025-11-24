using Conflux.Database;
using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Conflux.Services;

public class UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext DbContext) : IUserService {
    public Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal claimsPrincipal) {
        return userManager.GetUserAsync(claimsPrincipal);
    }
    
    public Task<bool> IsUserNameTaken(string username) {
        return DbContext.Users.AnyAsync(user => username.Equals(user.UserName, StringComparison.InvariantCultureIgnoreCase));
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