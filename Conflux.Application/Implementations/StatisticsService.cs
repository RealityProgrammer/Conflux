// ReSharper disable AccessToDisposedClosure

using Conflux.Application.Abstracts;
using Conflux.Application.Dto;
using Conflux.Domain;
using Conflux.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Conflux.Application.Implementations;

public sealed class StatisticsService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory
) : IStatisticsService
{
    public async Task<UserStatisticsDTO> GetUserStatistics() {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        // Get number of users.
        int userCount = await dbContext.Users.CountAsync();
        
        return new(userCount, 0, 0);
    }

    public async Task<ReportStatisticsDTO> GetReportStatistics() {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return await dbContext.MessageReports
            .Include(r => r.Message)
            .ThenInclude(m => m.Conversation)
            .Where(r => r.Message.Conversation.CommunityChannelId == null)
            .GroupBy(r => 1)
            .Select(g => new ReportStatisticsDTO(g.Count(), g.Count(r => r.Status != ReportStatus.InProgress)))
            .SingleAsync();
    }
    
    public async Task<ReportCountStatistics?> GetReportCountStatisticsAsync(Guid communityId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        var now = DateTime.UtcNow;
        var today = now.Date;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var statistics = await dbContext.MessageReports
            .Where(c => c.Message.Conversation.CommunityChannel!.ChannelCategory.CommunityId == communityId)
            .Select(r => new { r.CreatedAt, r.Status })
            .GroupBy(r => 1)
            .Select(g => new ReportCountStatistics(
                Total: g.Count(),
                Today: g.Count(x => x.CreatedAt >= today),
                ThisMonth: g.Count(x => x.CreatedAt >= startOfMonth),
                ThisYear: g.Count(x => x.CreatedAt >= startOfYear),
                Resolved: g.Count(x => x.Status != ReportStatus.InProgress)
            ))
            .SingleOrDefaultAsync();

        return statistics;
    }
    
    public async Task<UserReportStatistics?> GetUserReportStatistics(Guid userId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var stats = await dbContext.MessageReports
            .Where(r => r.Message.SenderId == userId && r.Message.Conversation.CommunityChannelId == null)
            .GroupBy(r => 1)
            .Select(g => new UserReportStatistics(
                TotalReportCount: g.Count(),
                ResolvedReportCount: g.Count(r => r.Status != ReportStatus.InProgress)
            ))
            .SingleOrDefaultAsync();

        return stats;
    }

    public async Task<UserReportStatistics?> GetMemberReportStatisticsAsync(Guid memberId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var extractedIds = await dbContext.CommunityMembers
            .Where(member => member.Id == memberId)
            .Select(member => new { member.UserId, member.CommunityId })
            .FirstOrDefaultAsync();

        if (extractedIds == null) {
            return null;
        }

        var stats = await dbContext.MessageReports
            .Where(r => r.Message.SenderId == extractedIds.UserId && r.Message.Conversation.CommunityChannel!.ChannelCategory.CommunityId == extractedIds.CommunityId)
            .GroupBy(r => 1)
            .Select(g => new UserReportStatistics(
                TotalReportCount: g.Count(),
                ResolvedReportCount: g.Count(r => r.Status != ReportStatus.InProgress)
            ))
            .SingleOrDefaultAsync();

        return stats;
    }
}