namespace Conflux.Database.Entities;

public enum ConversationType {
    DirectMessage,
    TextChannel,
}

public class Conversation : ICreatedAtColumn {
    public Guid Id { get; set; }
    public ConversationType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public ICollection<ChatMessage> Messages { get; set; } = null!;
    
    public Guid? FriendRequestId { get; set; }
    public FriendRequest? FriendRequest { get; set; }
    
    public Guid? CommunityChannelId { get; set; }
    public CommunityChannel? CommunityChannel { get; set; }
}