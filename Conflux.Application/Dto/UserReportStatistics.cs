namespace Conflux.Application.Dto;

public record struct UserReportStatistics(int TotalReportCount, int ResolvedReportCount, int WarnCount, int BanCount);