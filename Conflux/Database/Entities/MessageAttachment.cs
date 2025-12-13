using System.ComponentModel.DataAnnotations;

namespace Conflux.Database.Entities;

public enum MessageAttachmentType {
    Image,
    Audio,
    Video,
}

public class MessageAttachment {
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }

    [MaxLength(64)] public string Name { get; set; } = null!;
    [MaxLength(96)] public string PhysicalPath { get; set; } = null!;
    
    public MessageAttachmentType Type { get; set; }

    public ChatMessage Message { get; set; } = null!;
    
    // TODO: Make attachment removable.
}