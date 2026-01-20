namespace Conflux.Domain.Enums;

[Flags]
public enum AccessPermissionFlags {
    None = 0,
        
    AccessControlPanel = 1 << 0,
        
    All = AccessControlPanel,
}