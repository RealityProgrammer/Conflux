using System.ComponentModel.DataAnnotations;

namespace Conflux.Database.Entities;

public class Community : ICreatedAtColumn {
    public Guid Id { get; set; }
    
    [MaxLength(64)] public string Name { get; set; } = null!;
    [MaxLength(255)] public string? AvatarPath { get; set; }
    [MaxLength(255)] public string? BannerPath { get; set; }
    
    [MaxLength(36)] public string CreatorId { get; set; } = null!;

    public Guid InvitationId { get; set; }
    
    [MaxLength(255)] public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public ApplicationUser Creator { get; set; } = null!;

    public ICollection<CommunityMember> Members { get; set; } = null!;
    public IList<CommunityChannelCategory> ChannelCategories { get; set; } = null!;
    public ICollection<CommunityRole> Roles { get; set; } = null!;
}