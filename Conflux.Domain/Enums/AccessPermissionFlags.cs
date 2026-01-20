namespace Conflux.Domain.Enums;

[Flags]
public enum AccessPermissionFlags {
    None = 0,
        
    AccessControlPanel = 1 << 0,
    AccessReports = 1 << 1,
        
    All = AccessControlPanel | AccessReports,
}