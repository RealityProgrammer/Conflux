using Conflux.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Conflux.Services.Abstracts;

public interface IUserService {
    Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal claimsPrincipal);
    
    Task<bool> IsUserNameTaken(string username);
    
    Task<bool> IsTwoFactorEnabled(ClaimsPrincipal claimsPrincipal);
    Task<bool> IsTwoFactorEnabled(ApplicationUser user);

    Task<bool> IsProfileSetup(ClaimsPrincipal claimsPrincipal);
    Task UpdateProfileSetup(ClaimsPrincipal claimsPrincipal, bool value);

    Task<IdentityResult> AssignRoleAsync(ApplicationUser user, string roleName);
    Task<IdentityResult> RemoveRoleAsync(ApplicationUser user, string roleName);
    Task<IList<string>> GetRolesAsync(ApplicationUser user);
}