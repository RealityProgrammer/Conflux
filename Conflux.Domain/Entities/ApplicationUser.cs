using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Conflux.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>, ICreatedAtColumn {
    public bool IsProfileSetup { get; set; }
    
    [MinLength(8), MaxLength(32), Required] public required string DisplayName { get; set; }
    
    [MaxLength(32)] public string? Pronouns { get; set; }
    
    [MaxLength(255)] public string? Bio { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    [MaxLength(255)] public string? AvatarProfilePath { get; set; }

    [MaxLength(128)] public string? StatusText { get; set; }
    
    public ICollection<FriendRequest> SentFriendRequests { get; set; } = null!;
    public ICollection<FriendRequest> ReceivedFriendRequests { get; set; } = null!;

    public ICollection<Community> OwnedCommunities { get; set; } = null!;
    public ICollection<Community> CreatedCommunities { get; set; } = null!;
    public ICollection<CommunityMember> CommunityMembers { get; set; } = null!;
}