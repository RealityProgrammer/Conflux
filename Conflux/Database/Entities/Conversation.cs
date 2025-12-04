namespace Conflux.Database.Entities;

public class Conversation : ICreatedAtColumn {
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public required ICollection<ChatMessage> Messages { get; set; }
    public required ICollection<ConversationMember> Members { get; set; }
}