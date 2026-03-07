using Conflux.Domain.Enums;

namespace Conflux.Application.Dto;

public record RolePermissions(
    ChannelPermissions Channel,
    Domain.Enums.RolePermissions Role,
    AccessPermissions Access,
    ManagementPermissions Management
)
{
    public static RolePermissions Default { get; } = new(ChannelPermissions.None, Domain.Enums.RolePermissions.None, AccessPermissions.None, ManagementPermissions.None);
}