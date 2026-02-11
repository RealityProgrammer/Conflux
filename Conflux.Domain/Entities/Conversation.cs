namespace Conflux.Domain.Entities;

public class Conversation {
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? LatestMessageTime { get; set; }
    
    public ICollection<ChatMessage> Messages { get; set; } = null!;
    
    public Guid? FriendRequestId { get; set; }
    public FriendRequest? FriendRequest { get; set; }
    
    public Guid? CommunityChannelId { get; set; }
    public CommunityChannel? CommunityChannel { get; set; }
}