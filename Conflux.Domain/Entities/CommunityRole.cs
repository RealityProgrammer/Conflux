using Conflux.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Conflux.Domain.Entities;

public class CommunityRole
{
    public Guid Id { get; set; }

    [MaxLength(32)] public string Name { get; set; } = null!;

    public Guid CommunityId { get; set; }
    public Community Community { get; set; } = null!;

    public RolePermissions RolePermissions { get; set; }
    public ChannelPermissions ChannelPermissions { get; set; }
    public AccessPermissions AccessPermissions { get; set; }
    public ManagementPermissions ManagementPermissions { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<CommunityMember> MembersWithRole { get; set; } = null!;
}