namespace Conflux.Application.Dto;

public readonly record struct ReportStatisticsDTO(
    int TotalReportCount,
    int UnresolvedReportCount,
    int GlobalDismissCount,
    int GlobalWarnCount,
    int GlobalBanCount
);