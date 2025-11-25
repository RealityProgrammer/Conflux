using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conflux.Database.Entities;

public class FriendRequest : ICreatedAtColumn {
    [Required] public required string SenderId { get; set; }
    [Required] public required string ReceiverId { get; set; }
    [Required] public required FriendRequestStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime ResponseAt { get; set; }
    
    public required ApplicationUser Sender { get; init; }
    public required ApplicationUser Receiver { get; init; }
}