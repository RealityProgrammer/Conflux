using Conflux.Application.Dto;
using Conflux.Domain.Entities;
using Conflux.Domain.Enums;

namespace Conflux.Application.Abstracts;

public interface IReportService {
    Task<bool> ReportMessageAsync(Guid messageId, string? extraMessage, ReportReasons[] reasons, Guid reporterUserId);

    Task<ReportCountStatistics?> GetReportCountStatisticsAsync(Guid communityId);
    
    Task<MemberReportStatistics?> GetMemberReportStatisticsAsync(Guid memberId);
    
    Task<List<ReportedMessageDTO>> GetMemberReportedMessagesAsync(Guid memberId);

    Task<List<MessageReport>> GetMessageReportsAsync(Guid messageId);
    
    Task<MessageReportStatistics?> GetMessageReportStatisticsAsync(Guid messageId);
    
    Task<ReportDisplayDTO?> GetReportDisplayAsync(Guid reportId);

    Task<(int Count, List<UserDisplayDTO> Page)> PaginateReportedUsersAsync(int startIndex, int count);
    Task<(int Count, List<MemberDisplayDTO> Page)> PaginateReportedMembersAsync(Guid communityId, int startIndex, int count);

    Task<bool> ResolveReportByDismissAsync(Guid reportId, Guid resolverMemberId);
    
    Task<bool> ResolveReportByWarningAsync(Guid reportId, Guid resolverMemberId);
    
    Task<bool> ResolveReportByBanningAsync(Guid reportId, Guid resolverMemberId, TimeSpan banDuration);
}