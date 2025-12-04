using System.ComponentModel.DataAnnotations;

namespace Conflux.Database.Entities;

public class Message : ICreatedAtColumn {
    // TODO: Media (Image, Audio, File, etc...), Replying, Mentioning.
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    [MaxLength(36)] public required string SenderId { get; set; } = null!;
    
    [MaxLength(1024)] public required string Body { get; set; }
    
    public Guid? ReplyMessageId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Conversation Conversation { get; set; } = null!;
    public ApplicationUser Sender { get; set; } = null!;    // TODO: nagivate to ConversationMember instead?
    public Message? ReplyMessage { get; set; }
}