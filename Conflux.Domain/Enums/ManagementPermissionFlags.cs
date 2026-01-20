namespace Conflux.Domain.Enums;

[Flags]
public enum ManagementPermissionFlags {
    None = 0,
    
    ManageReports = 1 << 0,
    
    All = ManageReports,
}