namespace Conflux.Database.Entities;

public enum ConversationType {
    DirectMessage,
}

public class Conversation : ICreatedAtColumn {
    public Guid Id { get; set; }
    
    public ConversationType Type { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public ICollection<ChatMessage> Messages { get; set; } = null!;
    public ICollection<ConversationMember> Members { get; set; } = null!;
}