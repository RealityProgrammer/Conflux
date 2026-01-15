using Conflux.Domain.Entities;

namespace Conflux.Application.Dto;

public record struct MessageReportStatistics(int ReportCount, Dictionary<ReportReasons, int> ReasonCounts);