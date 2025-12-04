namespace Conflux.Database.Entities;

public class ConversationMember : ICreatedAtColumn {
    public required Guid ConversationId { get; set; }
    public required string UserId { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public Conversation Conversation { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}