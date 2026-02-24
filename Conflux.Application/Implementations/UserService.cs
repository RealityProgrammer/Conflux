using Conflux.Application.Abstracts;
using Conflux.Application.Dto;
using Conflux.Domain;
using Conflux.Domain.Entities;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Conflux.Application.Implementations;

public class UserService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    ICacheService cacheService
) : IUserService {
    public async Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal claimsPrincipal) {
        return await userManager.GetUserAsync(claimsPrincipal);
    }
    
    public async Task<bool> IsUserNameTaken(string username) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        return await dbContext.Users.AnyAsync(user => userManager.NormalizeName(username).Equals(user.NormalizedUserName, StringComparison.InvariantCulture));
    }

    public async Task<bool> IsUserEmailConfirmed(Guid userId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return await dbContext.Users.Where(u => u.Id == userId).Select(u => u.EmailConfirmed).FirstOrDefaultAsync();
    }

    public async Task<bool> IsTwoFactorEnabled(ClaimsPrincipal claimsPrincipal) {
        var user = await userManager.GetUserAsync(claimsPrincipal);
        
        if (user == null) return false;
        
        return await userManager.GetTwoFactorEnabledAsync(user);
    }

    public async Task<bool> IsTwoFactorEnabled(ApplicationUser user) {
        return await userManager.GetTwoFactorEnabledAsync(user);
    }

    public async Task<bool> IsProfileSetup(ClaimsPrincipal claimsPrincipal) {
        return await Task.FromResult(claimsPrincipal.FindFirstValue("ProfileSetup") == bool.TrueString);
    }

    public async Task UpdateProfileSetup(ClaimsPrincipal claimsPrincipal, bool value) {
        var user = await userManager.GetUserAsync(claimsPrincipal);

        if (user == null || user.IsProfileSetup == value) return;
        
        user.IsProfileSetup = value;
        
        await userManager.UpdateAsync(user);
        
        await signInManager.RefreshSignInAsync(user);
    }

    public Task<IdentityResult> AssignRoleAsync(ApplicationUser user, string roleName) {
        return userManager.AddToRoleAsync(user, roleName);
    }

    public Task<IdentityResult> RemoveRoleAsync(ApplicationUser user, string roleName) {
        return userManager.RemoveFromRoleAsync(user, roleName);
    }

    public async Task<IList<string>> GetRolesAsync(Guid userId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        return await dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(dbContext.Roles, 
                ur => ur.RoleId, 
                r => r.Id, 
                (ur, r) => r.Name!)
            .ToListAsync();
    }
    
    public async Task<UserDisplayDTO?> GetUserDisplayAsync(Guid userId) {
        return await cacheService.GetOrSetUserDisplayAsync(userId, RetrieveFromDatabase);

        async Task<UserDisplayDTO?> RetrieveFromDatabase(Guid id) {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return await dbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserDisplayDTO(u.Id, u.DisplayName, u.UserName, u.AvatarProfilePath))
                .Cast<UserDisplayDTO?>()
                .FirstOrDefaultAsync();
        }
    }

    public async Task<(int TotalCount, IReadOnlyList<UserDisplayDTO> Page)> PaginateUserDisplayAsync(
        Func<IQueryable<ApplicationUser>, IOrderedQueryable<ApplicationUser>> orderQueryProvider,
        Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>> filterQueryProvider,
        int start, 
        int count
    ) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var queryStatement = filterQueryProvider(orderQueryProvider(dbContext.Users));
        
        int userCount = await queryStatement.CountAsync();

        if (userCount == 0) {
            return (0, []);
        }
        
        List<UserDisplayDTO> page = await queryStatement
            .Select(u => new UserDisplayDTO(u.Id, u.DisplayName, u.UserName, u.AvatarProfilePath))
            .ToListAsync();
        
        return (userCount, page);
    }

    public async Task<UserBanState?> GetBanStateAsync(Guid userId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        return await dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserBanState(u.UnbanAt ?? DateTime.MinValue))
            .Cast<UserBanState?>()
            .FirstOrDefaultAsync();
    }

    public async Task<UserBanDetails?> GetLatestBanDetails(Guid userId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return await dbContext.ModerationRecords
            .Where(m => m.OffenderUserId == userId && m.Action == ModerationAction.Ban)
            .OrderByDescending(m => m.CreatedAt)
            .Take(1)
            .Include(m => m.OffenderUser)
            .Select(m => new UserBanDetails(m.Reason, m.BanDuration!.Value, m.OffenderUser.UnbanAt!.Value))
            .Cast<UserBanDetails?>()
            .FirstOrDefaultAsync();
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user) {
        var result = await userManager.UpdateAsync(user);

        if (result.Succeeded) {
            await cacheService.ResetUserDisplayAsync(user.Id);
        }

        return result;
    }
}