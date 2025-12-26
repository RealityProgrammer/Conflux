using Conflux.Database.Entities;

namespace Conflux.Services.Abstracts;

public interface ICommunityPermissionService {
    Task<Permissions?> GetPermissionsAsync(Guid roleId);
    Task<bool> UpdatePermissionsAsync(Guid roleId, Permissions permissions);
    
    public record Permissions(
        CommunityRole.ChannelPermissionFlags ChannelPermissions,
        CommunityRole.RolePermissionFlags RolePermissions
    );
}