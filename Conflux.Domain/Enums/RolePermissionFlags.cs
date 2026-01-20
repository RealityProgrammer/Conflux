namespace Conflux.Domain.Enums;

[Flags]
public enum RolePermissionFlags : byte {
    None = 0,
        
    CreateRole = 1 << 0,
    DeleteRole = 1 << 1,
    ModifyRolePermissions = 1 << 2,
    ModifyMemberRole = 1 << 3,
    RenameRole = 1 << 4,
        
    All = CreateRole | DeleteRole | ModifyRolePermissions | ModifyMemberRole | RenameRole,
}