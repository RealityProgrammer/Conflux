using Conflux.Application.Dto;

namespace Conflux.Application.Abstracts;

public interface ICommunityCacheService {
    Task<RolePermissions?> GetPermissionsAsync(Guid roleId);
    Task StorePermissionsAsync(Guid roleId, RolePermissions permissions);
    Task RemovePermissionsAsync(Guid roleId);
}