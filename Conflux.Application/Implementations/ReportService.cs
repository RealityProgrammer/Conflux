using Conflux.Application.Abstracts;
using Conflux.Application.Dto;
using Conflux.Domain;
using Conflux.Domain.Entities;
using Conflux.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Conflux.Application.Implementations;

public class ReportService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    ICommunityService communityService,
    ILogger<ReportService> logger
) : IReportService {
    public async Task<bool> ReportMessageAsync(Guid messageId, string? extraMessage, ReportReasons[] reasons, Guid reporterUserId) {
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
            ReporterUserId = reporterUserId,
            OriginalMessageBody = messageData.Body,
            OriginalMessageAttachments = messageData.Attachments,
            CreatedAt = DateTime.UtcNow,
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
            .SingleOrDefaultAsync();

        return statistics;
    }

    public async Task<(int Count, List<UserDisplayDTO> Page)> PaginateReportedUsersAsync(int startIndex, int count) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var reportedUserIds = dbContext.MessageReports
            .Include(r => r.Message)
            .ThenInclude(m => m.Conversation)
            .Where(r => r.Message.Conversation.CommunityChannelId == null)
            .Select(r => r.Message.SenderId)
            .Distinct();

        int reportedUsersCount = await reportedUserIds.CountAsync();

        if (reportedUsersCount == 0) {
            return (0, []);
        }

        var users = await reportedUserIds
            .Join(dbContext.Users, id => id, user => user.Id, (id, user) => user)
            .OrderBy(u => u.DisplayName)
            .Skip(startIndex)
            .Take(count)
            .Select(u => new UserDisplayDTO(u.Id, u.DisplayName, u.UserName, u.AvatarProfilePath))
            .ToListAsync();

        return (reportedUsersCount, users);
    }

    public async Task<(int Count, List<MemberDisplayDTO> Page)> PaginateReportedMembersAsync(Guid communityId, int startIndex, int count) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var reportedMemberIds = QueryMessageReportsFromCommunity(dbContext, communityId)
            .Select(r => r.Message.SenderId)
            .Distinct();
        
        int reportedMembersCount = await reportedMemberIds.CountAsync();

        if (reportedMembersCount == 0) {
            return (0, []);
        }

        var members = await reportedMemberIds
            .Join(dbContext.CommunityMembers, id => id, member => member.Id, (id, member) => member)
            .Include(member => member.User)
            .OrderBy(member => member.User.DisplayName)
            .Skip(startIndex)
            .Take(count)
            .Select(member => new MemberDisplayDTO(member.Id, member.User.Id, member.User.DisplayName, member.User.AvatarProfilePath))
            .ToListAsync();

        return (reportedMembersCount, members);
    }

    public async Task<MemberReportStatistics?> GetMemberReportStatisticsAsync(Guid memberId) {
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
            .Select(g => new MemberReportStatistics(
                TotalReportCount: g.Count(),
                ResolvedReportCount: g.Count(r => r.Status != ReportStatus.InProgress)
            ))
            .SingleOrDefaultAsync();

        return stats;
    }

    public async Task<List<ReportedMessageDTO>> GetMemberReportedMessagesAsync(Guid memberId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var extractedIds = await dbContext.CommunityMembers
            .Where(member => member.Id == memberId)
            .Select(member => new { member.UserId, member.CommunityId })
            .FirstOrDefaultAsync();

        if (extractedIds == null) {
            return [];
        }

        var reports = await QueryMessageReportsFromCommunity(dbContext, extractedIds.CommunityId)
            .Include(r => r.Message)
            .Where(r => r.Message.SenderId == extractedIds.UserId)
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
            .SingleOrDefaultAsync();

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
            .Include(r => r.Message)
            .ThenInclude(m => m.Sender)
            .Include(r => r.Reporter)
            .Select(r => new ReportDisplayDTO(r.Id, r.Message.Sender.DisplayName, r.Message.Sender.AvatarProfilePath, r.OriginalMessageBody, r.OriginalMessageAttachments, r.ReporterUserId, r.CreatedAt, r.Reasons, r.ExtraMessage, r.Status, r.ResolverId, r.ResolvedAt, r.BanDuration))
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
        if (banDuration < TimeSpan.Zero) {
            return false;
        }
        
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var extractedIds = await dbContext.MessageReports
            .Where(r => r.Id == reportId && r.Status == ReportStatus.InProgress)
            .Include(r => r.Message)
            .ThenInclude(r => r.Conversation)
            .ThenInclude(r => r.CommunityChannel!)
            .ThenInclude(r => r.ChannelCategory)
            .Include(r => r.Message)
            .Select(r => new {
                r.Message.Conversation.CommunityChannel!.ChannelCategory.CommunityId,
                r.Message.SenderId,
            })
            .FirstOrDefaultAsync();

        if (extractedIds == null) {
            return false;
        }
        
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        
        var utcNow = DateTime.UtcNow;
        
        int affected = await dbContext.MessageReports
            .Where(r => r.Id == reportId && r.Status == ReportStatus.InProgress)
            .ExecuteUpdateAsync(builder => {
                builder.SetProperty(r => r.Status, ReportStatus.Banned);
                builder.SetProperty(r => r.ResolverId, resolverMemberId);
                builder.SetProperty(r => r.ResolvedAt, utcNow);
                builder.SetProperty(r => r.BanDuration, banDuration);
            });

        if (affected > 0) {
            var memberId = await dbContext.CommunityMembers
                .Where(m => m.CommunityId == extractedIds.CommunityId && m.UserId == extractedIds.SenderId)
                .Select(m => m.Id)
                .FirstAsync();

            if (await communityService.BanMemberAsync(dbContext, extractedIds.CommunityId, memberId, banDuration)) {
                await transaction.CommitAsync();
                return true;
            }
        }

        return false;
    }

    private static IQueryable<MessageReport> QueryMessageReportsFromCommunity(ApplicationDbContext context, Guid communityId) {
        return context.MessageReports.Include(r => r.Message)
            .ThenInclude(m => m.Conversation)
            .ThenInclude(c => c.CommunityChannel!)
            .ThenInclude(c => c.ChannelCategory)
            .Where(c => c.Message.Conversation.CommunityChannel!.ChannelCategory.CommunityId == communityId);
    }
}