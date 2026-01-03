using Conflux.Database.Entities;

namespace Conflux.Core;
    
public record RolePermissions(
    CommunityRole.ChannelPermissionFlags Channel,
    CommunityRole.RolePermissionFlags Role,
    CommunityRole.AccessPermissionFlags Access
);