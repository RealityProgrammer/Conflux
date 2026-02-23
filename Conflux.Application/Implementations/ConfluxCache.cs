using Conflux.Application.Abstracts;
using Conflux.Application.Dto;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Conflux.Application.Implementations;

public sealed class ConfluxCache(IDistributedCache cache) : IConfluxCache {
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

    public async Task<UserDisplayDTO?> GetOrSetUserDisplayAsync<TState>(Guid userId, Func<Guid, TState, Task<UserDisplayDTO?>> factory, TState state) {
        string key = CreateUserDisplayCacheKey(userId);
        var cachedData = await cache.GetAsync(key);

        if (cachedData != null) {
            return JsonSerializer.Deserialize<UserDisplayDTO>(cachedData);
        }

        var factoryResult = await factory(userId, state);

        if (factoryResult == null) {
            return null;
        }

        await using var memoryStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(memoryStream, factoryResult);
        
        await cache.SetAsync(key, memoryStream.ToArray());

        return factoryResult;
    }

    private string CreateUserDisplayCacheKey(Guid userId) {
        return $"users.display.{userId:N}";
    }
}