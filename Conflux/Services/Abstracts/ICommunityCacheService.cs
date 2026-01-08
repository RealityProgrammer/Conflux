using Conflux.Core;
using Conflux.Database.Entities;

namespace Conflux.Services.Abstracts;

public interface ICommunityCacheService {
    Task<RolePermissions?> GetPermissionsAsync(Guid roleId);
    Task StorePermissionsAsync(Guid roleId, RolePermissions permissions);
    Task RemovePermissionsAsync(Guid roleId);
}