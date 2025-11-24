using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Conflux.Database.Entities;

public class ApplicationUser : IdentityUser, ICreatedAtColumn {
    public bool IsProfileSetup { get; set; }
    
    [MinLength(8), MaxLength(32), Required] public required string DisplayName { get; set; }
    
    [MaxLength(32)] public string? Pronouns { get; set; }
    
    [MaxLength(255)] public string? Bio { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // TODO: Question ourself why is this needed while the avatars always at "images/avatar/{Id}"
    [MaxLength(255)] public string? ProfilePicturePath { get; set; }

    [Range(0.25, 5)] public double ProfilePictureScaleX { get; set; } = 1;
    [Range(0.25, 5)] public double ProfilePictureScaleY { get; set; } = 1;
    
    [MaxLength(255)] public string? BannerPicturePath { get; set; }
    
    [MaxLength(128)] public string? StatusText { get; set; }
    
    public ICollection<FriendRequest> SentFriendRequests { get; }
    public ICollection<FriendRequest> ReceivedFriendRequests { get; }
}