using Conflux.Application.Dto;

namespace Conflux.Application.Abstracts;

public interface ICacheService {
    Task<UserDisplayDTO?> GetOrSetUserDisplayAsync(Guid userId, Func<Guid, Task<UserDisplayDTO?>> factory);
    Task<UserDisplayDTO?> GetOrSetUserDisplayAsync<TState>(Guid userId, Func<Guid, TState, Task<UserDisplayDTO?>> factory, TState state);

    Task<TData> GetOrSetStatisticsDataAsync<TData>(string key, Func<Task<TData>> factory);
}