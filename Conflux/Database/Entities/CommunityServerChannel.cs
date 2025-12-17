using System.ComponentModel.DataAnnotations;

namespace Conflux.Database.Entities;

public class CommunityServerChannel : ICreatedAtColumn {
    public Guid Id { get; set; }

    [MaxLength(32)] public string Name { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    
    public Guid ServerChannelCategoryId { get; set; }

    public CommunityServerChannelCategory ServerChannelCategory { get; set; } = null!;
}