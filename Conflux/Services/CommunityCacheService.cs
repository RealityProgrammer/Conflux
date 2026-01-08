using Conflux.Core;
using Conflux.Services.Abstracts;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

namespace Conflux.Services;

public class CommunityCacheService(
    IMemoryCache cache
) : ICommunityCacheService {
    private static readonly TimeSpan PermissionCacheDuration = TimeSpan.FromMinutes(5);
    
    public Task<RolePermissions?> GetPermissionsAsync(Guid roleId) {
        return Task.FromResult(cache.Get<RolePermissions?>(GeneratePermissionCacheKey(roleId)));
    }

    public Task StorePermissionsAsync(Guid roleId, RolePermissions permissions) {
        cache.Set(GeneratePermissionCacheKey(roleId), permissions, PermissionCacheDuration);
        return Task.CompletedTask;
    }

    public Task RemovePermissionsAsync(Guid roleId) {
        cache.Remove(GeneratePermissionCacheKey(roleId));
        return Task.CompletedTask;
    }

    private static string GeneratePermissionCacheKey(Guid roleId) {
        // Structure: CommunityRoles.<Role ID Format N>.Permissions
        return string.Create("CommunityRoles.".Length + 32 + ".Permissions".Length, roleId, CreateCallback);

        static void CreateCallback(Span<char> buffer, Guid state) {
            "CommunityRoles.".CopyTo(buffer);
            
            bool formatSuccessful = state.TryFormat(buffer.Slice("CommunityRoles.".Length, 32), out _, "N");
            Debug.Assert(formatSuccessful);
            
            ".Permissions".CopyTo(buffer[("CommunityRoles.".Length + 32)..]);
        }
    }
}