using Conflux.Database;
using Conflux.Services.Abstracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

namespace Conflux.Services;

public sealed class CommunityPermissionService(
    IMemoryCache memoryCache,
    IDbContextFactory<ApplicationDbContext> dbContextFactory
) : ICommunityPermissionService {
    
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    
    public async Task<ICommunityPermissionService.Permissions?> GetPermissionsAsync(Guid roleId) {
        string cacheKey = GeneratePermissionCacheKey(roleId);
        
        if (memoryCache.TryGetValue<ICommunityPermissionService.Permissions>(cacheKey, out var cached)) {
            return cached!;
        }

        var permissions = await CollectPermissions(roleId);

        if (permissions == null) {
            return null;
        }
        
        memoryCache.Set(cacheKey, permissions, CacheDuration);

        return permissions;
    }

    public async Task<bool> UpdatePermissionsAsync(Guid roleId, ICommunityPermissionService.Permissions permissions) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        int numUpdatedRows = await dbContext.CommunityRoles
            .Where(x => x.Id == roleId)
            .ExecuteUpdateAsync(builder => {
                builder.SetProperty(x => x.ChannelPermissions, permissions.ChannelPermissions);
                builder.SetProperty(x => x.RolePermissions, permissions.RolePermissions);
            });

        if (numUpdatedRows == 0) {
            return false;
        }
        
        string cacheKey = GeneratePermissionCacheKey(roleId);
        
        memoryCache.Set(cacheKey, permissions, CacheDuration);

        return true;
    }

    private async Task<ICommunityPermissionService.Permissions?> CollectPermissions(Guid roleId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return await dbContext.CommunityRoles
            .Where(x => x.Id == roleId)
            .Select(x => new ICommunityPermissionService.Permissions(x.ChannelPermissions, x.RolePermissions))
            .FirstOrDefaultAsync();
    }

    private static string GeneratePermissionCacheKey(Guid roleId) {
        return string.Create("CommunityRoles.".Length + 32 + ".Permissions".Length, roleId, CreateCallback);

        static void CreateCallback(Span<char> buffer, Guid state) {
            "CommunityRoles.".CopyTo(buffer);
            bool formatSuccessful = state.TryFormat(buffer.Slice("CommunityRoles.".Length, 32), out _, "N");
            Debug.Assert(formatSuccessful);
            ".Permissions".CopyTo(buffer[("CommunityRoles.".Length + 32)..]);
        }
    }
}