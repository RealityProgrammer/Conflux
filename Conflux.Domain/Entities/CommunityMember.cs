using System.ComponentModel.DataAnnotations;

namespace Conflux.Domain.Entities;

public class CommunityMember : ICreatedAtColumn {
    public Guid Id { get; set; }
    
    public Guid CommunityId { get; set; }
    public Community Community { get; set; } = null!;
    
    [MaxLength(36)] public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    
    public Guid? RoleId { get; set; }
    public CommunityRole? Role { get; set; }
    
    public DateTime CreatedAt { get; set; }
}