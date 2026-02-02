namespace Conflux.Domain.Entities;

public class FriendRequest : ICreatedAtColumn {
    public Guid Id { get; set; }
    
    public Guid SenderId { get; set; }
    public ApplicationUser Sender { get; set; } = null!;
    
    public Guid ReceiverId { get; set; }
    public ApplicationUser Receiver { get; set; } = null!;
    
    public Conversation? Conversation { get; set; }
    
    public FriendRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResponseAt { get; set; }
}