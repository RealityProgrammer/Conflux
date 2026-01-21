using System.ComponentModel.DataAnnotations;

namespace Conflux.Domain.Entities;

public class ChatMessage : ICreatedAtColumn {
    // TODO: Media (Image, Audio, File, etc...), Replying, Mentioning.
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    
    public required Guid SenderId { get; set; }
    
    [MaxLength(1024)] public string? Body { get; set; }
    
    public Guid? ReplyMessageId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public List<MessageAttachment> Attachments { get; set; } = null!;

    public Conversation Conversation { get; set; } = null!;
    public ApplicationUser Sender { get; set; } = null!;
    public ChatMessage? ReplyMessage { get; set; }

    public ICollection<MessageReport> Reports { get; set; } = null!;
}