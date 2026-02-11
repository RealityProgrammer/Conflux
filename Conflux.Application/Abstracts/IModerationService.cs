using Conflux.Application.Dto;
using Conflux.Domain.Entities;
using Conflux.Domain.Enums;

namespace Conflux.Application.Abstracts;

public interface IModerationService {
    Task<bool> ReportMessageAsync(Guid messageId, string? extraMessage, ReportReasons[] reasons, Guid reporterUserId);

    Task<List<MessageReport>> GetMessageReportsAsync(Guid messageId);
    
    Task<MessageReportStatistics?> GetMessageReportReasonCounts(Guid messageId);
    
    Task<ReportDisplayDTO?> GetReportDisplayAsync(Guid reportId);

    Task<(int TotalCount, List<UserDisplayDTO> Page)> PaginateReportedUsersAsync(int startIndex, int count);
    Task<(int TotalCount, List<Guid>)> PaginateUserReportedMessageIdsAsync(Guid userId, int startIndex, int count);
    Task<(int TotalCount, List<Guid>)> PaginateReportIdsAsync(Guid messageId, int startIndex, int count);
    
    Task<(int Count, List<MemberDisplayDTO> Page)> PaginateReportedMembersAsync(Guid communityId, int startIndex, int count);
    Task<(int Count, List<Guid>)> PaginateMemberReportedMessageIdsAsync(Guid memberId, int startIndex, int count);
    
    Task<bool> ResolveReportByDismissAsync(Guid reportId, Guid resolverUserId);
    
    Task<bool> ResolveReportByWarningAsync(Guid reportId, Guid resolverUserId);
    
    Task<bool> ResolveReportByBanningAsync(Guid reportId, Guid resolverUserId, TimeSpan banDuration);

    Task<bool> WarnUserAsync(Guid userId);
    Task<bool> BanUserAsync(Guid userId, TimeSpan duration);
}