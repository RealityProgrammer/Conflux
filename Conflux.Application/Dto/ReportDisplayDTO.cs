using Conflux.Domain.Entities;

namespace Conflux.Application.Dto;

public record struct ReportDisplayDTO(
    string SenderDisplayName, 
    string? SenderAvatar, 
    string? OriginalBody, 
    List<MessageAttachment> OriginalAttachments, 
    string ReporterId, 
    DateTime CreatedAt, 
    ReportReasons[] Reasons,
    string? ExtraMessage,
    ReportStatus Status
);