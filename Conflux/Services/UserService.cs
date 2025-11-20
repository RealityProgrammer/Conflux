using Conflux.Database;
using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Conflux.Services;

public class UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext DbContext) : IUserService {
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
}