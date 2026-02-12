using Conflux.Domain.Enums;

namespace Conflux.Application.Dto;

public record struct ModerationRecordDisplayDTO(
    Guid OffenderUserId,
    UserDisplayDTO ResolverUser,
    Guid? MessageReportId,
    ModerationAction Action,
    TimeSpan? BanDuration,
    string? Reason,
    DateTime CreatedAt
);