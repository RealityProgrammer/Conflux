using Conflux.Application.Abstracts;
using Conflux.Application.Dto;
using Conflux.Domain;
using Conflux.Domain.Entities;
using Conflux.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Conflux.Application.Implementations;

public class ModerationService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    ICommunityService communityService,
    ILogger<ModerationService> logger
) : IModerationService {
    public async Task<bool> ReportMessageAsync(Guid messageId, string? extraMessage, ReportReason[] reasons, Guid reporterUserId) {
        if (reasons.Length == 0) {
            return false;
        }
        
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var messageData = await dbContext.ChatMessages
            .Where(m => m.Id == messageId)
            .Select(m => new {
                SenderId = m.SenderUserId, m.Body, m.Attachments })
            .FirstOrDefaultAsync();

        if (messageData == null) {
            return false;
        }
        
        dbContext.Add(new MessageReport {
            MessageId = messageId,
            ExtraMessage = extraMessage,
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

    public async Task<(int TotalCount, List<UserDisplayDTO> Page)> PaginateReportedUsersAsync(int startIndex, int count) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var reportedUserIds = dbContext.MessageReports
            .Include(r => r.Message)
            .ThenInclude(m => m.Conversation)
            .Where(r => r.Message.Conversation.CommunityChannelId == null)
            .Select(r => r.Message.SenderUserId)
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

    public async Task<(int TotalCount, List<Guid>)> PaginateUserReportedMessageIdsAsync(Guid userId, int startIndex, int count) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var query = dbContext.MessageReports
            .Include(r => r.Message)
            .ThenInclude(m => m.Conversation)
            .Where(r => r.Message.Conversation.CommunityChannelId == null && r.Message.SenderUserId == userId)
            .Select(r => r.MessageId);

        var totalCount = await query.CountAsync();

        if (totalCount == 0) {
            return (0, []);
        }

        var page = await query
            .Skip(startIndex)
            .Take(count)
            .ToListAsync();

        return (totalCount, page);
    }

    public async Task<(int TotalCount, List<Guid>)> PaginateReportIdsAsync(Guid messageId, int startIndex, int count) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var query = dbContext.MessageReports
            .Where(r => r.MessageId == messageId);

        var totalCount = await query.CountAsync();

        if (totalCount == 0) {
            return (0, []);
        }

        var page = await query
            .OrderBy(r => r.ModerationRecordId == null ? 1 : 0)
            .Skip(startIndex)
            .Take(count)
            .Select(r => r.Id)
            .ToListAsync();

        return (totalCount, page);
    }

    public async Task<(int Count, List<MemberDisplayDTO> Page)> PaginateReportedMembersAsync(Guid communityId, int startIndex, int count) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        var reportedUserIds = QueryMessageReportsFromCommunity(dbContext, communityId)
            .Select(r => r.Message.SenderUserId)
            .Distinct();
        
        int reportedMembersCount = await reportedUserIds.CountAsync();
        
        if (reportedMembersCount == 0) {
            return (0, []);
        }

        var users = await reportedUserIds
            .Join(
                dbContext.CommunityMembers.Where(m => m.CommunityId == communityId), 
                id => id,
                member => member.UserId,
                (id, member) => member
            )
            .Include(m => m.User)
            .OrderBy(m => m.User.DisplayName)
            .Skip(startIndex)
            .Take(count)
            .Select(m => new MemberDisplayDTO(m.Id, new(m.UserId, m.User.DisplayName, m.User.UserName, m.User.AvatarProfilePath)))
            .ToListAsync();

        return (reportedMembersCount, users);
    }

    public async Task<(int Count, List<Guid>)> PaginateMemberReportedMessageIdsAsync(Guid memberId, int startIndex, int count) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var extractedIds = await dbContext.CommunityMembers
            .Where(member => member.Id == memberId)
            .Select(member => new { member.UserId, member.CommunityId })
            .FirstOrDefaultAsync();
        
        if (extractedIds == null) {
            return (0, []);
        }

        var query = QueryMessageReportsFromCommunity(dbContext, extractedIds.CommunityId)
            .Include(r => r.Message)
            .Where(r => r.Message.SenderUserId == extractedIds.UserId)
            .Select(r => r.MessageId)
            .Distinct();
            
        int totalCount = await query.CountAsync();

        if (totalCount == 0) {
            return (0, []);
        }
        
        var page = await query
            .OrderBy(g => g)
            .Skip(startIndex)
            .Take(count)
            .ToListAsync();

        return (totalCount, page);
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

    public async Task<MessageReportStatistics?> GetMessageReportReasonCounts(Guid messageId) {
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
            .Select(r => new ReportDisplayDTO(r.Id, r.Message.Sender.DisplayName, r.Message.Sender.AvatarProfilePath, r.OriginalMessageBody, r.OriginalMessageAttachments, r.ReporterUserId, r.CreatedAt, r.Reasons, r.ExtraMessage, r.ModerationRecordId))
            .Cast<ReportDisplayDTO?>()
            .FirstOrDefaultAsync();
    }

    public async Task<ModerationRecordDisplayDTO?> GetModerationRecordDisplayAsync(Guid recordId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return await dbContext.ModerationRecords
            .Where(r => r.Id == recordId)
            .Include(r => r.ResolverUser)
            .Select(r => 
                new ModerationRecordDisplayDTO(
                    OffenderUserId: r.OffenderUserId, 
                    ResolverUser: new(r.ResolverUser.Id, r.ResolverUser.DisplayName, r.ResolverUser.UserName, r.ResolverUser.AvatarProfilePath), 
                    MessageReportId: r.MessageReportId,
                    Action: r.Action,
                    BanDuration: r.BanDuration,
                    Reason: r.Reason,
                    CreatedAt: r.CreatedAt
                )
            )
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ResolveReportByDismissAsync(Guid reportId, Guid resolverUserId, string? reason) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        Guid? nullableUserId = await dbContext.MessageReports
            .Where(r => r.Id == reportId && r.ModerationRecordId == null)
            .Select(r => r.Message.SenderUserId)
            .Cast<Guid?>()
            .FirstOrDefaultAsync();

        if (nullableUserId is not { } userId) {
            return false;
        }
        
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try {
            var record = new ModerationRecord {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                ResolverUserId = resolverUserId,
                MessageReportId = reportId,
                Action = ModerationAction.Dismiss,
                BanDuration = null,
                Reason = reason,
                OffenderUserId = userId,
            };

            dbContext.ModerationRecords.Add(record);
            await dbContext.SaveChangesAsync();
            
            int affected = await dbContext.MessageReports
                .Where(r => r.Id == reportId && r.ModerationRecordId == null)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.ModerationRecordId, record.Id));

            if (affected == 0) {
                await transaction.RollbackAsync();
                return false;
            }

            await transaction.CommitAsync();
            return true;
        } catch {
            await transaction.RollbackAsync();
            return false;
        }
    }
    
    public async Task<bool> ResolveReportByWarningAsync(Guid reportId, Guid resolverUserId, string? reason) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        Guid? nullableUserId = await dbContext.MessageReports
            .Where(r => r.Id == reportId && r.ModerationRecordId == null)
            .Select(r => r.Message.SenderUserId)
            .Cast<Guid?>()
            .FirstOrDefaultAsync();

        if (nullableUserId is not { } userId) {
            return false;
        }
        
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try {
            var record = new ModerationRecord {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                ResolverUserId = resolverUserId,
                MessageReportId = reportId,
                Action = ModerationAction.Warn,
                BanDuration = null,
                Reason = reason,
                OffenderUserId = userId,
            };

            dbContext.ModerationRecords.Add(record);
            await dbContext.SaveChangesAsync();
            
            int affected = await dbContext.MessageReports
                .Where(r => r.Id == reportId && r.ModerationRecordId == null)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.ModerationRecordId, record.Id));

            if (affected == 0) {
                await transaction.RollbackAsync();
                return false;
            }

            await transaction.CommitAsync();
            return true;
        } catch {
            await transaction.RollbackAsync();
            return false;
        }
    }
    
    public async Task<bool> ResolveReportByBanningAsync(Guid reportId, Guid resolverUserId, TimeSpan banDuration, string? reason) {
        if (banDuration < TimeSpan.Zero) {
            return false;
        }
        
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var extractedIds = await dbContext.MessageReports
            .Where(r => r.Id == reportId && r.ModerationRecordId == null)
            .Include(r => r.Message)
            .ThenInclude(r => r.Conversation)
            .ThenInclude(r => r.CommunityChannel!)
            .ThenInclude(r => r.ChannelCategory)
            .Include(r => r.Message)
            .Select(r => new {
                r.Message.Conversation.CommunityChannel!.ChannelCategory.CommunityId,
                SenderId = r.Message.SenderUserId,
            })
            .FirstOrDefaultAsync();

        if (extractedIds == null) {
            return false;
        }
        
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try {
            var record = new ModerationRecord {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                ResolverUserId = resolverUserId,
                MessageReportId = reportId,
                Action = ModerationAction.Warn,
                BanDuration = banDuration,
                Reason = reason,
                OffenderUserId = extractedIds.SenderId,
            };
            
            dbContext.ModerationRecords.Add(record);
            await dbContext.SaveChangesAsync();
            
            int affected = await dbContext.MessageReports
                .Where(r => r.Id == reportId && r.ModerationRecordId == null)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.ModerationRecordId, record.Id));

            if (affected == 0) {
                await transaction.RollbackAsync();
                return false;
            }

            var memberId = await dbContext.CommunityMembers
                .Where(m => m.CommunityId == extractedIds.CommunityId && m.UserId == extractedIds.SenderId)
                .Select(m => m.Id)
                .FirstAsync();
            
            if (await communityService.BanMemberAsync(dbContext, extractedIds.CommunityId, memberId, banDuration)) {
                await transaction.CommitAsync();
                return true;
            }

            return false;
        } catch {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> WarnUserAsync(Guid userId, Guid resolverUserId, string? reason) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        var record = new ModerationRecord {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            ResolverUserId = resolverUserId,
            Action = ModerationAction.Warn,
            BanDuration = null,
            Reason = reason,
            OffenderUserId = userId,
        };

        dbContext.ModerationRecords.Add(record);
        return await dbContext.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> BanUserAsync(Guid userId, Guid resolverUserId, TimeSpan duration, string? reason) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        var record = new ModerationRecord {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            ResolverUserId = resolverUserId,
            Action = ModerationAction.Warn,
            BanDuration = duration,
            Reason = reason,
            OffenderUserId = userId,
        };

        dbContext.ModerationRecords.Add(record);
        return await dbContext.SaveChangesAsync() > 0;
    }

    private static IQueryable<MessageReport> QueryMessageReportsFromCommunity(ApplicationDbContext context, Guid communityId) {
        return context.MessageReports
            .Include(r => r.Message)
            .ThenInclude(m => m.Conversation)
            .ThenInclude(c => c.CommunityChannel!)
            .ThenInclude(c => c.ChannelCategory)
            .Where(c => c.Message.Conversation.CommunityChannel!.ChannelCategory.CommunityId == communityId);
    }
}