using Conflux.Application.Dto;
using Conflux.Domain.Entities;
using Conflux.Domain.Enums;

namespace Conflux.Application.Abstracts;

public interface IReportService {
    Task<bool> ReportMessageAsync(Guid messageId, string? extraMessage, ReportReasons[] reasons, Guid reporterUserId);

    Task<List<Guid>> GetMemberReportedMessageIdentitiesAsync(Guid memberId);
    Task<List<Guid>> GetUserReportedMessageIdentitiesAsync(Guid userId);

    Task<List<MessageReport>> GetMessageReportsAsync(Guid messageId);
    
    Task<MessageReportStatistics?> GetMessageReportStatisticsAsync(Guid messageId);
    
    Task<ReportDisplayDTO?> GetReportDisplayAsync(Guid reportId);

    Task<(int TotalCount, List<UserDisplayDTO> Page)> PaginateReportedUsersAsync(int startIndex, int count);
    Task<(int TotalCount, List<Guid>)> PaginateUserReportedMessageIdsAsync(Guid userId, int startIndex, int count);
    Task<(int TotalCount, List<Guid>)> PaginateReportIdsAsync(Guid messageId, int startIndex, int count);
    
    Task<(int Count, List<MemberDisplayDTO> Page)> PaginateReportedMembersAsync(Guid communityId, int startIndex, int count);

    Task<bool> ResolveReportByDismissAsync(Guid reportId, Guid resolverMemberId);
    
    Task<bool> ResolveReportByWarningAsync(Guid reportId, Guid resolverMemberId);
    
    Task<bool> ResolveReportByBanningAsync(Guid reportId, Guid resolverMemberId, TimeSpan banDuration);
}