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
}