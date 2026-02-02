namespace Conflux.Domain.Enums;

[Flags]
public enum ManagementPermissionFlags {
    None = 0,
    
    ManageReports = 1 << 0,
    DeleteMemberMessage = 1 << 1,
    BanMember = 1 << 2,
    
    All = ManageReports,
}