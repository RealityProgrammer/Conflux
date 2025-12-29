using System.ComponentModel.DataAnnotations;

namespace Conflux.Database.Entities;

public class CommunityRole : ICreatedAtColumn {
    public Guid Id { get; set; }

    [MaxLength(32)] public string Name { get; set; } = null!;
    
    public Guid CommunityId { get; set; }
    public Community Community { get; set; } = null!;
    
    public RolePermissionFlags RolePermissions { get; set; }
    public ChannelPermissionFlags ChannelPermissions { get; set; }
    public AccessPermissionFlags AccessPermissions { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public ICollection<CommunityMember> MembersWithRole { get; set; } = null!;

    [Flags]
    public enum RolePermissionFlags : byte {
        None = 0,
        
        CreateRole = 1 << 0,
        DeleteRole = 1 << 1,
        ModifyRolePermissions = 1 << 2,
        ModifyMemberRole = 1 << 3,
        
        All = CreateRole | DeleteRole | ModifyRolePermissions | ModifyMemberRole,
    }
    
    [Flags]
    public enum ChannelPermissionFlags : byte {
        None = 0,
    
        CreateChannelCategory = 1 << 0,
        DeleteChannelCategory = 1 << 1,
    
        CreateChannel = 1 << 2,
        DeleteChannel = 1 << 3,
        
        All = CreateChannelCategory | DeleteChannelCategory | CreateChannel | DeleteChannel,
    }

    [Flags]
    public enum AccessPermissionFlags {
        None = 0,
        
        AccessControlPanel = 1 << 0,
        
        All = AccessControlPanel,
    }
}