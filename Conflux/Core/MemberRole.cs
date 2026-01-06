namespace Conflux.Core;

public record MemberRolePermissions(Guid? RoleId, RolePermissions Permissions) {
    public static MemberRolePermissions Default { get; } = new(null, RolePermissions.Default);
}