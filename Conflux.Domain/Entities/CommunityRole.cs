using Conflux.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Conflux.Domain.Entities;

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
}