using Conflux.Application.Dto;
using Conflux.Domain;
using Conflux.Domain.Entities;
using Conflux.Domain.Enums;

namespace Conflux.Application.Abstracts;

public interface IModerationService {
    Task<bool> ReportMessageAsync(Guid messageId, string? extraMessage, ReportReason[] reasons, Guid reporterUserId);

    Task<List<MessageReport>> GetMessageReportsAsync(Guid messageId);
    
    Task<MessageReportStatistics?> GetMessageReportReasonCounts(Guid messageId);
    
    Task<ReportDisplayDTO?> GetReportDisplayAsync(Guid reportId);
    Task<ModerationRecordDisplayDTO?> GetModerationRecordDisplayAsync(Guid recordId);

    Task<(int TotalCount, List<UserDisplayDTO> Page)> PaginateReportedUsersAsync(Func<IQueryable<MessageReport>, IQueryable<MessageReport>> filterQueryProvider, int startIndex, int count);
    Task<(int TotalCount, List<Guid>)> PaginateUserReportedMessageIdsAsync(Guid userId, int startIndex, int count);
    Task<(int TotalCount, List<Guid>)> PaginateReportIdsAsync(Guid messageId, int startIndex, int count);
    
    Task<(int Count, List<MemberDisplayDTO> Page)> PaginateReportedMembersAsync(Guid communityId, int startIndex, int count);
    Task<(int Count, List<Guid>)> PaginateMemberReportedMessageIdsAsync(Guid memberId, int startIndex, int count);
    
    Task<bool> ResolveReportByDismissAsync(Guid reportId, Guid resolverUserId, string? reason);
    Task<bool> ResolveReportByWarningAsync(Guid reportId, Guid resolverUserId, string? reason);
    Task<bool> ResolveReportByBanningAsync(Guid reportId, Guid resolverUserId, TimeSpan banDuration, string? reason);

    Task<bool> WarnUserAsync(Guid userId, Guid resolverUserId, string? reason);
    Task<bool> BanUserAsync(Guid userId, Guid resolverUserId, TimeSpan duration, string? reason);
    
    Task<bool> WarnMemberAsync(Guid memberId, Guid resolverUserId, string? reason);
    Task<bool> BanMemberAsync(Guid memberId, Guid resolverUserId, TimeSpan duration, string? reason);
}