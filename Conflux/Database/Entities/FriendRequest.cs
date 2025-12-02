using System.ComponentModel.DataAnnotations;

namespace Conflux.Database.Entities;

public class FriendRequest : ICreatedAtColumn {
    [Required] public required string SenderId { get; set; }
    [Required] public required string ReceiverId { get; set; }
    [Required] public required FriendRequestStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ResponseAt { get; set; }
    
    public ApplicationUser Sender { get; set; } = null!;
    public ApplicationUser Receiver { get; set; } = null!;

    [Timestamp] public byte[] Version { get; set; } = null!;
}