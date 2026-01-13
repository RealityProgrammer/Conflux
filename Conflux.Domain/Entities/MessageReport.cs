using System.ComponentModel.DataAnnotations;

namespace Conflux.Domain.Entities;

public class MessageReport : ICreatedAtColumn {
    public Guid Id { get; set; }
    
    public Guid MessageId { get; set; }
    public ChatMessage Message { get; set; } = null!;

    [Required] public ReportReasons[] Reasons { get; set; } = null!;
    
    [MaxLength(255)] public string? ExtraMessage { get; set; }

    [Required, MaxLength(36)] public string ReporterId { get; set; } = null!;
    public ApplicationUser Reporter { get; set; } = null!;
    
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}