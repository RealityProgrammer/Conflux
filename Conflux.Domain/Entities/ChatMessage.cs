using System.ComponentModel.DataAnnotations;

namespace Conflux.Domain.Entities;

public class ChatMessage {
    public Guid Id { get; set; }
    
    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;
    
    public Guid SenderUserId { get; set; }
    public ApplicationUser Sender { get; set; } = null!;
    
    [MaxLength(1024)] public string? Body { get; set; }
    
    public Guid? ReplyMessageId { get; set; }
    public ChatMessage? ReplyMessage { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public Guid? DeleterUserId { get; set; }
    public ApplicationUser? DeleterUser { get; set; }

    public List<MessageAttachment> Attachments { get; set; } = null!;
    
    public ICollection<MessageReport> Reports { get; set; } = null!;
}