using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Conflux.Database.Entities;

public sealed class Friendship : ICreatedAtColumn {
    [Required] public required string UserId { get; set; }
    [Required] public required string FriendId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public required ApplicationUser User { get; init; }
    public required ApplicationUser Friend { get; init; }
}