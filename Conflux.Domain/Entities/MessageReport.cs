using Conflux.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Conflux.Domain.Entities;

public class MessageReport : ICreatedAtColumn {
    public Guid Id { get; set; }
    
    public Guid MessageId { get; set; }
    public ChatMessage Message { get; set; } = null!;

    // TODO: Reduce redundancy when report same message multiple time
    [MaxLength(1024)] public string? OriginalMessageBody { get; set; }
    public List<MessageAttachment> OriginalMessageAttachments { get; set; } = null!;
    
    [Required] public ReportReasons[] Reasons { get; set; } = null!;
    
    [MaxLength(255)] public string? ExtraMessage { get; set; }

    public Guid ReporterUserId { get; set; }
    public ApplicationUser Reporter { get; set; } = null!;

    public Guid? ResolverUserId { get; set; }
    public ApplicationUser? ResolverUser { get; set; }
    
    public DateTime? ResolvedAt { get; set; }
    
    public TimeSpan? BanDuration { get; set; }
    
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}