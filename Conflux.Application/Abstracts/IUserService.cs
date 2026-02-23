using Conflux.Application.Dto;
using Conflux.Domain;
using Conflux.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Conflux.Application.Abstracts;

public interface IUserService {
    Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal claimsPrincipal);
    
    Task<bool> IsUserNameTaken(string username);
    Task<bool> IsUserEmailConfirmed(Guid userId);
    
    Task<bool> IsTwoFactorEnabled(ClaimsPrincipal claimsPrincipal);
    Task<bool> IsTwoFactorEnabled(ApplicationUser user);

    Task<bool> IsProfileSetup(ClaimsPrincipal claimsPrincipal);
    Task UpdateProfileSetup(ClaimsPrincipal claimsPrincipal, bool value);

    Task<IdentityResult> AssignRoleAsync(ApplicationUser user, string roleName);
    Task<IdentityResult> RemoveRoleAsync(ApplicationUser user, string roleName);
    Task<IList<string>> GetRolesAsync(Guid userId);
    
    Task<UserDisplayDTO?> GetUserDisplayAsync(Guid userId);
    
    Task<UserBanState?> GetBanStateAsync(Guid userId);
    Task<UserBanDetails?> GetLatestBanDetails(Guid userId);

    Task<(int TotalCount, IReadOnlyList<UserDisplayDTO> Page)> PaginateUserDisplayAsync(
        Func<IQueryable<ApplicationUser>, IOrderedQueryable<ApplicationUser>> orderQueryProvider,
        Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>> filterQueryProvider,
        int start, 
        int count
    );
}