using System.ComponentModel.DataAnnotations;

namespace Conflux.Domain.Entities;

public class ModerationRecord {
    public Guid Id { get; set; }
    
    public Guid? ResolverUserId { get; set; }
    public ApplicationUser? ResolverUser { get; set; }
    
    public Guid? MessageReportId { get; set; }
    public MessageReport? MessageReport { get; set; }
    
    public TimeSpan? BanDuration { get; set; }
    [MaxLength(512)] public string? Reason { get; set; }
    
    public DateTime CreatedAt { get; set; }
}