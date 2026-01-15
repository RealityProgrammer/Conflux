using Conflux.Application.Abstracts;
using Conflux.Application.Dto;
using Conflux.Domain;
using Conflux.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Conflux.Application.Implementations;

public class ReportService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory
) : IReportService {
    public async Task<bool> ReportMessageAsync(Guid messageId, string? extraMessage, ReportReasons[] reasons, string reporterId) {
        if (reasons.Length == 0) {
            return false;
        }
        
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var messageData = await dbContext.ChatMessages
            .Where(m => m.Id == messageId)
            .Select(m => new { m.SenderId, m.Body, m.Attachments })
            .FirstOrDefaultAsync();

        if (messageData == null) {
            return false;
        }
        
        dbContext.Add(new MessageReport {
            MessageId = messageId,
            ExtraMessage = extraMessage,
            Status = ReportStatus.InProgress,
            Reasons = reasons,
            ReporterId = reporterId,
            MessageSenderId = messageData.SenderId,
            OriginalMessageBody = messageData.Body,
            OriginalMessageAttachments = messageData.Attachments,
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
                Resolved: g.Count(x => x.Status != ReportStatus.InProgress)
            ))
            .FirstOrDefaultAsync();

        return statistics;
    }

    public async Task<(int Count, List<MemberDisplayDTO> Page)> PaginateReportedMembersAsync(Guid communityId, int startIndex, int count) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var reportedUsers = QueryMessageReportsFromCommunity(dbContext, communityId)
            .Select(r => r.Message.SenderId)
            .Distinct();
        
        int reportedUsersCount = await reportedUsers.CountAsync();

        if (reportedUsersCount == 0) {
            return (0, []);
        }

        var members = await dbContext.CommunityMembers
            .Where(member => reportedUsers.Contains(member.UserId))
            .Include(member => member.User)
            .OrderBy(member => member.User.DisplayName)
            .Skip(startIndex)
            .Take(count)
            .Select(member => new MemberDisplayDTO(member.Id, member.User.Id, member.User.DisplayName, member.User.AvatarProfilePath))
            .ToListAsync();

        return (reportedUsersCount, members);
    }

    public async Task<MemberReportStatistics?> GetMemberReportStatisticsAsync(Guid communityId, Guid memberId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var userId = await dbContext.CommunityMembers
            .Where(member => member.Id == memberId && member.CommunityId == communityId)
            .Select(member => member.UserId)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(userId)) {
            return null;
        }

        var stats = await dbContext.MessageReports
            .Where(r => r.Message.SenderId == userId && r.Message.Conversation.CommunityChannel!.ChannelCategory.CommunityId == communityId)
            .GroupBy(r => 1)
            .Select(g => new MemberReportStatistics(
                TotalReportCount: g.Count(),
                ResolvedReportCount: g.Count(r => r.Status != ReportStatus.InProgress)
            ))
            .FirstOrDefaultAsync();

        return stats;
    }

    public async Task<List<ReportedMessageDTO>> GetMemberReportedMessagesAsync(Guid communityId, Guid memberId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var userId = await dbContext.CommunityMembers
            .Where(member => member.Id == memberId && member.CommunityId == communityId)
            .Select(member => member.UserId)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(userId)) {
            return [];
        }
        
        // This cannot be compiled by EntityFramework for some reason...
        // var reports = await QueryMessageReportsFromCommunity(dbContext, communityId)
        //     .Where(r => r.MessageSenderId == userId)
        //     .Select(r => new MessageReportListingElementDTO {
        //         CreatedAt = r.CreatedAt,
        //         Id = r.Id,
        //         MessageId = r.MessageId,
        //     })
        //     .GroupBy(r => r.MessageId, (_, g) => g.OrderByDescending(x => x.CreatedAt).First())
        //     .OrderByDescending(c => c.CreatedAt)
        //     .ToListAsync();
        //
        // return reports;

        var reports = await QueryMessageReportsFromCommunity(dbContext, communityId)
            .Where(r => r.MessageSenderId == userId)
            .Select(r => new ReportedMessageDTO() {
                CreatedAt = r.CreatedAt,
                Id = r.Id,
                MessageId = r.MessageId,
            })
            .GroupBy(r => r.MessageId)
            .Select(g => new { g.Key, Candidate = g.OrderByDescending(x => x.CreatedAt).FirstOrDefault() })
            .ToListAsync();
        
        return reports.Select(r => r.Candidate).OrderByDescending(c => c.CreatedAt).ToList();
    }

    public async Task<List<MessageReport>> GetMessageReportsAsync(Guid messageId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        return await dbContext.MessageReports
            .Where(r => r.MessageId == messageId)
            .OrderByDescending(r => r.CreatedAt)
            .Include(r => r.Message)
            .Include(r => r.Reporter)
            .ToListAsync();
    }

    public async Task<MessageReportStatistics?> GetMessageReportStatisticsAsync(Guid messageId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var result = await dbContext.MessageReports
            .Where(r => r.MessageId == messageId)
            .GroupBy(r => 1)
            .Select(group =>
                new {
                    ReportCount = group.Count(),
                    ReasonsCount = group.SelectMany(report => report.Reasons)
                        .GroupBy(reason => reason)
                        .Select(g => new {
                            g.Key,
                            Count = g.Count(),
                        }),
                }
            )
            .FirstOrDefaultAsync();

        if (result == null) {
            return null;
        }

        return new(result.ReportCount, result.ReasonsCount.ToDictionary(r => r.Key, r => r.Count));
    }
    
    public async Task<ReportDisplayDTO?> GetReportDisplayAsync(Guid reportId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        return await dbContext.MessageReports
            .Where(r => r.Id == reportId)
            .Include(r => r.MessageSender)
            .Include(r => r.Reporter)
            .Select(r => new ReportDisplayDTO(r.Id, r.MessageSender.DisplayName, r.MessageSender.AvatarProfilePath, r.OriginalMessageBody, r.OriginalMessageAttachments, r.ReporterId, r.CreatedAt, r.Reasons, r.ExtraMessage, r.Status, r.ResolverId, r.ResolvedAt))
            .Cast<ReportDisplayDTO?>()
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ResolveReportByDismissAsync(Guid reportId, Guid resolverMemberId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        var utcNow = DateTime.UtcNow;

        int affected = await dbContext.MessageReports
            .Where(r => r.Id == reportId && r.Status == ReportStatus.InProgress)
            .ExecuteUpdateAsync(builder => {
                builder.SetProperty(r => r.Status, ReportStatus.Dismissed);
                builder.SetProperty(r => r.ResolverId, resolverMemberId);
                builder.SetProperty(r => r.ResolvedAt, utcNow);
            });

        return affected > 0;
    }
    
    public async Task<bool> ResolveReportByWarningAsync(Guid reportId, Guid resolverMemberId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        var utcNow = DateTime.UtcNow;

        int affected = await dbContext.MessageReports
            .Where(r => r.Id == reportId && r.Status == ReportStatus.InProgress)
            .ExecuteUpdateAsync(builder => {
                builder.SetProperty(r => r.Status, ReportStatus.Warned);
                builder.SetProperty(r => r.ResolverId, resolverMemberId);
                builder.SetProperty(r => r.ResolvedAt, utcNow);
            });

        return affected > 0;
    }
    
    public async Task<bool> ResolveReportByBanningAsync(Guid reportId, Guid resolverMemberId, TimeSpan banDuration) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        var utcNow = DateTime.UtcNow;
        
        int affected = await dbContext.MessageReports
            .Where(r => r.Id == reportId && r.Status == ReportStatus.InProgress)
            .ExecuteUpdateAsync(builder => {
                builder.SetProperty(r => r.Status, ReportStatus.Banned);
                builder.SetProperty(r => r.ResolverId, resolverMemberId);
                builder.SetProperty(r => r.ResolvedAt, utcNow);
            });

        return affected > 0;
    }

    private static IQueryable<MessageReport> QueryMessageReportsFromCommunity(ApplicationDbContext context, Guid communityId) {
        return context.MessageReports.Include(r => r.Message)
            .ThenInclude(m => m.Conversation)
            .ThenInclude(c => c.CommunityChannel!)
            .ThenInclude(c => c.ChannelCategory)
            .Where(c => c.Message.Conversation.CommunityChannel!.ChannelCategory.CommunityId == communityId);
    }
}