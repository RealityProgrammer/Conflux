using Conflux.Database.Entities;

namespace Conflux.Services.Abstracts;

public interface ICurrentUserService {
    Task<ApplicationUser?> GetCurrentUserAsync();

    Task RefreshUserAsync();
    Task UpdateUserAsync();
    
    Task LogoutAsync();
}