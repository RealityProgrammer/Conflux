using Conflux.Application.Abstracts;
using Conflux.Application.Dto;
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

    public async Task<ReportCountStatistics?> GetReportCountStatisticsAsync(Guid communityId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        var now = DateTime.UtcNow;
        var today = now.Date;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var statistics = await QueryMessageReportsFromCommunity(dbContext, communityId)
            .Select(r => new { r.CreatedAt, r.Status })
            .GroupBy(r => 1)
            .Select(g => new ReportCountStatistics(
                Total: g.Count(),
                Today: g.Count(x => x.CreatedAt >= today),
                ThisMonth: g.Count(x => x.CreatedAt >= startOfMonth),
                ThisYear: g.Count(x => x.CreatedAt >= startOfYear),
                Resolved: g.Count(x => x.Status == ReportStatus.Resolved)
            ))
            .FirstOrDefaultAsync();

        return statistics;
    }

    public async Task<(int Count, List<MemberDisplayDTO> Page)> PaginateReportedMembersAsync(Guid communityId, int startIndex, int count) {
        throw new NotImplementedException();

        // return await QueryMessageReportsFromCommunity(dbContext, communityId)
        //     .Select(r => r.Message.SenderId)
        //     .Distinct()
        //     .ToListAsync();
    }

    private static IQueryable<MessageReport> QueryMessageReportsFromCommunity(ApplicationDbContext context, Guid communityId) {
        return context.MessageReports.Include(r => r.Message)
            .ThenInclude(m => m.Conversation)
            .ThenInclude(c => c.CommunityChannel!)
            .ThenInclude(c => c.ChannelCategory)
            .AsSplitQuery()
            .Where(c => c.Message.Conversation.CommunityChannel!.ChannelCategory.CommunityId == communityId);
    }
}