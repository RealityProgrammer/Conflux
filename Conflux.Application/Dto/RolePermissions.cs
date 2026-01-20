using Conflux.Domain.Enums;

namespace Conflux.Application.Dto;

public record RolePermissions(
    ChannelPermissionFlags Channel,
    RolePermissionFlags Role,
    AccessPermissionFlags Access,
    ManagementPermissionFlags Management
) {
    public static RolePermissions Default { get; } = new(ChannelPermissionFlags.None, RolePermissionFlags.None, AccessPermissionFlags.None, ManagementPermissionFlags.None);
}