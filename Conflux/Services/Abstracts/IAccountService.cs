using Conflux.Database.Entities;
using System.Security.Claims;

namespace Conflux.Services.Abstracts;

public interface IAccountService {
    Task<ApplicationUser?> GetCurrentUserAsync();

    Task<bool> IsTwoFactorEnabled();
    
    Task<bool> IsTwoFactorEnabled(ClaimsPrincipal claimsPrincipal);
    Task<bool> IsTwoFactorEnabled(ApplicationUser user);

    Task<bool> IsProfileSetup();
    
    Task<bool> IsProfileSetup(ClaimsPrincipal claimsPrincipal);

    Task MarkProfileSetup();
    Task MarkProfileSetup(ClaimsPrincipal claimsPrincipal);
}