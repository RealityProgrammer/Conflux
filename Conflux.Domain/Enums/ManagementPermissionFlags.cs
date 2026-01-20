namespace Conflux.Domain.Enums;

[Flags]
public enum ManagementPermissionFlags {
    None = 0,
    
    ProcessReport = 1 << 0,
    
    All = ProcessReport,
}