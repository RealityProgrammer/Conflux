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
    
    public async Task<ICommunityPermissionService.Permissions?> GetPermissionsAsync(Guid roleId) {
        Span<char> cacheKey = stackalloc char["CommunityRoles.".Length + 32 + ".Permissions".Length];
        GeneratePermissionCacheKey(cacheKey, roleId);

        string strCacheKey = cacheKey.ToString();
        
        if (memoryCache.TryGetValue<ICommunityPermissionService.Permissions>(strCacheKey, out var cached)) {
            return cached!;
        }

        var permissions = await CollectPermissions(roleId);

        if (permissions == null) {
            return null;
        }
        
        memoryCache.Set(strCacheKey, permissions, TimeSpan.FromMinutes(5));

        return permissions;
    }

    public async Task<bool> UpdatePermissionsAsync(Guid roleId, ICommunityPermissionService.Permissions permissions) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        int numUpdatedRows = await dbContext.CommunityRoles
            .Where(x => x.Id == roleId)
            .ExecuteUpdateAsync(builder => {
                builder.SetProperty(x => x.ChannelPermissions, permissions.ChannelPermissions);
            });

        if (numUpdatedRows == 0) {
            return false;
        }
        
        Span<char> cacheKey = stackalloc char["CommunityRoles.".Length + 32 + ".Permissions".Length];
        GeneratePermissionCacheKey(cacheKey, roleId);
        
        memoryCache.Remove(cacheKey.ToString());

        return true;
    }

    private async Task<ICommunityPermissionService.Permissions?> CollectPermissions(Guid roleId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return await dbContext.CommunityRoles
            .Where(x => x.Id == roleId)
            .Select(x => new ICommunityPermissionService.Permissions(x.ChannelPermissions))
            .FirstOrDefaultAsync();
    }

    private static void GeneratePermissionCacheKey(Span<char> buffer, Guid roleId) {
        Debug.Assert(buffer.Length == "CommunityRoles.".Length + 32 + ".Permissions".Length);
        
        "CommunityRoles.".CopyTo(buffer);
        bool formatSuccessful = roleId.TryFormat(buffer.Slice("CommunityRoles.".Length, 32), out _, "N");
        Debug.Assert(formatSuccessful);
        ".Permissions".CopyTo(buffer[("CommunityRoles.".Length + 32)..]);
    }
}