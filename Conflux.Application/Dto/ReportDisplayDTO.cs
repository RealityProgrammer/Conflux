using Conflux.Domain.Entities;
using Conflux.Domain.Enums;

namespace Conflux.Application.Dto;

public record struct ReportDisplayDTO(
    Guid Id,
    string SenderDisplayName, 
    string? SenderAvatar, 
    string? OriginalBody, 
    List<MessageAttachment> OriginalAttachments, 
    string ReporterId, 
    DateTime CreatedAt, 
    ReportReasons[] Reasons,
    string? ExtraMessage,
    ReportStatus Status,
    Guid? ResolverId,
    DateTime? ResolvedAt,
    TimeSpan? BanDuration
);