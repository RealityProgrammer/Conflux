using Conflux.Database.Entities;
using System.Security.Claims;

namespace Conflux.Services.Abstracts;

public interface IUserService {
    Task<bool> IsUserNameTaken(string username);
    
    Task<bool> IsTwoFactorEnabled(ClaimsPrincipal claimsPrincipal);
    Task<bool> IsTwoFactorEnabled(ApplicationUser user);

    Task<bool> IsProfileSetup(ClaimsPrincipal claimsPrincipal);
}