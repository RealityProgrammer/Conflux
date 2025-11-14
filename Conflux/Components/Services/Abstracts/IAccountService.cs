using Conflux.Database;
using Conflux.Database.Entities;
using System.Security.Claims;

namespace Conflux.Components.Services.Abstracts;

public interface IAccountService {
    Task<ApplicationUser?> GetCurrentUserAsync();
    
    Task<bool> IsTwoFactorEnabled();
    Task<bool> IsTwoFactorEnabled(ClaimsPrincipal claimsPrincipal);
    Task<bool> IsTwoFactorEnabled(ApplicationUser user);
}