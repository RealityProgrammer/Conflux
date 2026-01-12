using System.ComponentModel.DataAnnotations;

namespace Conflux.Domain.Entities;

public class MessageReport : ICreatedAtColumn {
    public Guid Id { get; set; }
    
    public Guid MessageId { get; set; }
    public ChatMessage Message { get; set; } = null!;

    [Required] public ReportReasons[] Reasons { get; set; } = null!;
    
    [MaxLength(255)] public string? OtherMessage { get; set; }
    
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}