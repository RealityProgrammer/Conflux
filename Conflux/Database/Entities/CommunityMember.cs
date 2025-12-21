using System.ComponentModel.DataAnnotations;

namespace Conflux.Database.Entities;

public class CommunityMember : ICreatedAtColumn {
    public Guid Id { get; set; }
    
    public Guid CommunityId { get; set; }
    [MaxLength(36)] public string UserId { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    
    public Community Community { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}