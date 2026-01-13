namespace Conflux.Application.Dto;

public record RolePermissionsWithId(Guid? RoleId, RolePermissions Permissions) {
    public static RolePermissionsWithId Default { get; } = new(null, RolePermissions.Default);
}