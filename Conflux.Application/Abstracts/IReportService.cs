using Conflux.Domain.Entities;

namespace Conflux.Application.Abstracts;

public interface IReportService {
    Task<bool> ReportMessageAsync(Guid messageId, string? extraMessage, ReportReasons[] reasons);
}