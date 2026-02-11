using Conflux.Domain.Entities;
using Conflux.Domain.Enums;

namespace Conflux.Application.Dto;

public record struct ReportDisplayDTO(
    Guid Id,
    string SenderDisplayName, 
    string? SenderAvatar, 
    string? OriginalBody, 
    List<MessageAttachment> OriginalAttachments, 
    Guid ReporterUserId, 
    DateTime CreatedAt, 
    ReportReason[] Reasons,
    string? ExtraMessage,
    Guid? ModerationRecordId
);