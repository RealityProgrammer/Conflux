using Conflux.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Conflux.Domain.Entities;

public class CommunityChannel : ICreatedAtColumn {
    public Guid Id { get; set; }

    [MaxLength(32)] public string Name { get; set; } = null!;
    
    public Guid ChannelCategoryId { get; set; }
    public CommunityChannelCategory ChannelCategory { get; set; } = null!;
    
    public CommunityChannelType Type { get; set; }
    
    public Conversation Conversation { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}