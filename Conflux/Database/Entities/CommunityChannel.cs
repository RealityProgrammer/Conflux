using System.ComponentModel.DataAnnotations;

namespace Conflux.Database.Entities;

public enum CommunityChannelType {
    Text,
    Audio,
}

public class CommunityChannel : ICreatedAtColumn {
    public Guid Id { get; set; }

    [MaxLength(32)] public string Name { get; set; } = null!;
    public Guid ChannelCategoryId { get; set; }
    public CommunityChannelType Type { get; set; }
    
    public Guid ConversationId { get; set; }

    public DateTime CreatedAt { get; set; }
    
    public CommunityChannelCategory ChannelCategory { get; set; } = null!;
    public Conversation Conversation { get; set; } = null!;
}