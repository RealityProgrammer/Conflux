using Conflux.Application.Abstracts;
using Conflux.Application.Dto;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Conflux.Application.Implementations;

public sealed class CacheService(IDistributedCache cache) : ICacheService {
    public async Task<UserDisplayDTO?> GetOrSetUserDisplayAsync(Guid userId, Func<Guid, Task<UserDisplayDTO?>> factory) {
        string key = CreateUserDisplayCacheKey(userId);
        var cachedData = await cache.GetAsync(key);

        if (cachedData != null) {
            return JsonSerializer.Deserialize<UserDisplayDTO>(cachedData);
        }

        var factoryResult = await factory(userId);

        if (factoryResult == null) {
            return null;
        }

        await using var memoryStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(memoryStream, factoryResult);
        
        await cache.SetAsync(key, memoryStream.ToArray());

        return factoryResult;
    }

    public async Task<RolePermissions?> GetOrSetCommunityRolePermissionsAsync(Guid roleId, Func<Guid, Task<RolePermissions?>> factory) {
        string key = CreateCommunityRolePermissionsCacheKey(roleId);
        var cachedData = await cache.GetAsync(key);

        if (cachedData != null) {
            return JsonSerializer.Deserialize<RolePermissions>(cachedData);
        }

        var factoryResult = await factory(roleId);

        if (factoryResult == null) {
            return null;
        }

        await using var memoryStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(memoryStream, factoryResult);
        
        await cache.SetAsync(key, memoryStream.ToArray());

        return factoryResult;
    }

    public async Task UpdateCommunityRolePermissionsAsync(Guid roleId, RolePermissions permissions) {
        string key = CreateCommunityRolePermissionsCacheKey(roleId);
        
        await using var memoryStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(memoryStream, permissions);
        
        await cache.SetAsync(key, memoryStream.ToArray());
    }

    public async Task<TData> GetOrSetStatisticsDataAsync<TData>(string key, Func<Task<TData>> factory) {
        key = CreateStatisticsDataAsync(key);
        var cachedData = await cache.GetAsync(key);

        if (cachedData != null) {
            return JsonSerializer.Deserialize<TData>(cachedData)!;
        }

        var factoryResult = await factory();

        if (factoryResult == null) {
            return default!;
        }

        await using var memoryStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(memoryStream, factoryResult);
        
        await cache.SetAsync(key, memoryStream.ToArray());

        return factoryResult;
    }
    
    private static string CreateUserDisplayCacheKey(Guid userId) {
        return $"user.display.{userId:N}";
    }
    
    private static string CreateCommunityRolePermissionsCacheKey(Guid roleId) {
        return $"community.role.permissions.{roleId:N}";
    }
    
    private static string CreateStatisticsDataAsync(string key) {
        return $"statistic.{key}";
    }
}