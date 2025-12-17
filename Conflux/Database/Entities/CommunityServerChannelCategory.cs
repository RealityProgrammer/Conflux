using System.ComponentModel.DataAnnotations;

namespace Conflux.Database.Entities;

public class CommunityServerChannelCategory : ICreatedAtColumn {
    public Guid Id { get; set; }

    [MaxLength(32)] public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    
    public Guid CommunityServerId { get; set; }

    public CommunityServer CommunityServer { get; set; } = null!;
    public ICollection<CommunityServerChannel> Channels { get; set; } = null!;
}