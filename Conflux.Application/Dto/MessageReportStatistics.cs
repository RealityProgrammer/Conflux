using Conflux.Domain.Entities;
using Conflux.Domain.Enums;

namespace Conflux.Application.Dto;

public record struct MessageReportStatistics(int ReportCount, Dictionary<ReportReason, int> ReasonCounts);