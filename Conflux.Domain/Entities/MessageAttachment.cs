using Conflux.Domain.Enums;

namespace Conflux.Domain.Entities;

public class MessageAttachment {
    public string Name { get; set; } = null!;
    public string PhysicalPath { get; set; } = null!;
    public MessageAttachmentType Type { get; set; }
}