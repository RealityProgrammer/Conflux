using Conflux.Domain.Entities;

namespace Conflux.Core;

public record RolePermissions(
    CommunityRole.ChannelPermissionFlags Channel,
    CommunityRole.RolePermissionFlags Role,
    CommunityRole.AccessPermissionFlags Access
) {
    public static RolePermissions Default { get; } = new(CommunityRole.ChannelPermissionFlags.None, CommunityRole.RolePermissionFlags.None, CommunityRole.AccessPermissionFlags.None);
}