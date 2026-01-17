using Conflux.Application.Dto;
using Conflux.Domain;

namespace Conflux.Application.Abstracts;

public interface ICommunityRoleService {
    Task<CreateRoleStatus> CreateRoleAsync(Guid communityId, string roleName);
    
    Task<bool> RenameRoleAsync(Guid communityId, Guid roleId, string name);
    
    Task<bool> DeleteRoleAsync(Guid communityId, Guid roleId);

    Task<bool> UpdatePermissionsAsync(Guid roleId, RolePermissions permissions);
    
    Task<RolePermissions?> GetPermissionsAsync(Guid roleId);
    Task<RolePermissions?> GetPermissionsAsync(ApplicationDbContext dbContext, Guid roleId);
    
    public enum CreateRoleStatus {
        Success,
        Failure,
        NameExists,
        ReservedName,
    }
}