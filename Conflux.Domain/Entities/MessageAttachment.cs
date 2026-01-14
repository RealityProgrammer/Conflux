namespace Conflux.Domain.Entities;

public enum MessageAttachmentType {
    Image,
    Audio,
    Video,
}

public class MessageAttachment {
    public string Name { get; set; } = null!;
    public string PhysicalPath { get; set; } = null!;
    public MessageAttachmentType Type { get; set; }
}