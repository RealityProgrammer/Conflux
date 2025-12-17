using System.ComponentModel.DataAnnotations;

namespace Conflux.Database.Entities;

public class CommunityChannelCategory : ICreatedAtColumn {
    public Guid Id { get; set; }

    [MaxLength(32)] public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    
    public Guid CommunityServerId { get; set; }

    public Community Community { get; set; } = null!;
    public ICollection<ComunityChannel> Channels { get; set; } = null!;
}