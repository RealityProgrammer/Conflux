using Conflux.Application.Dto;

namespace Conflux.Application.Abstracts;

public interface ICacheService {
    Task<UserDisplayDTO?> GetOrSetUserDisplayAsync(Guid userId, Func<Guid, Task<UserDisplayDTO?>> factory);
    Task ResetUserDisplayAsync(Guid userId);

    Task<RolePermissions?> GetOrSetCommunityRolePermissionsAsync(Guid roleId, Func<Guid, Task<RolePermissions?>> factory);
    Task UpdateCommunityRolePermissionsAsync(Guid roleId, RolePermissions permissions);
    
    Task<TData> GetOrSetStatisticsDataAsync<TData>(string key, Func<Task<TData>> factory);
}