using System.ComponentModel.DataAnnotations;

namespace Conflux.Domain.Entities;

public class CommunityMember : ICreatedAtColumn {
    public Guid Id { get; set; }
    
    public Guid CommunityId { get; set; }
    public Community Community { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    
    public Guid? RoleId { get; set; }
    public CommunityRole? Role { get; set; }
    
    public DateTime? UnbanAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
}