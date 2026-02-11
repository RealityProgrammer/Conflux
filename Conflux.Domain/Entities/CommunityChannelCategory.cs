using System.ComponentModel.DataAnnotations;

namespace Conflux.Domain.Entities;

public class CommunityChannelCategory {
    public Guid Id { get; set; }

    [MaxLength(32)] public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    
    public Guid CommunityId { get; set; }

    public Community Community { get; set; } = null!;
    public ICollection<CommunityChannel> Channels { get; set; } = null!;
}