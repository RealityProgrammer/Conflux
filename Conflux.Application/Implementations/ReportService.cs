using Conflux.Application.Abstracts;
using Conflux.Domain;
using Conflux.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Conflux.Application.Implementations;

public class ReportService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory
) : IReportService {
    public async Task<bool> ReportMessageAsync(Guid messageId, string? extraMessage, ReportReasons[] reasons) {
        if (reasons.Length == 0) {
            return false;
        }
        
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        dbContext.Add(new MessageReport {
            MessageId = messageId,
            ExtraMessage = extraMessage,
            Status = ReportStatus.InProgress,
            Reasons = reasons,
        });

        if (await dbContext.SaveChangesAsync() > 0) {
            return true;
        }

        return false;
    }
}