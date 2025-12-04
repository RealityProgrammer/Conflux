namespace Conflux.Database.Entities;

public enum ConversationType {
    DirectMessage,
    GroupMessage,
    Server,
}

public class Conversation : ICreatedAtColumn {
    public Guid Id { get; set; }
    
    public ConversationType Type { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public required ICollection<Message> Messages { get; set; }
    public required ICollection<ConversationMember> Members { get; set; }
    public required ICollection<Conversation> DirectConversations { get; set; }
}