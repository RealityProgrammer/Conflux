using Conflux.Application.Dto;

namespace Conflux.Application.Abstracts;

public interface IStatisticsService {
    Task<UserStatisticsDTO> GetUserStatistics();
    Task<ReportStatisticsDTO> GetReportStatistics();
    Task<ConversationStatisticsDTO> GetConversationStatistics();

    Task<ReportCountStatistics?> GetReportCountStatisticsAsync(Guid communityId);
    Task<UserReportStatistics?> GetUserReportStatistics(Guid userId);
    Task<UserReportStatistics?> GetMemberReportStatisticsAsync(Guid memberId);
}