using System.ComponentModel.DataAnnotations;

namespace Conflux.Database.Entities;

public class ComunityChannel : ICreatedAtColumn {
    public Guid Id { get; set; }

    [MaxLength(32)] public string Name { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    
    public Guid ServerChannelCategoryId { get; set; }

    public CommunityChannelCategory ChannelCategory { get; set; } = null!;
}