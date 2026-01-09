using Conflux.Database;
using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Conflux.Services;

public sealed partial class FriendshipService : IFriendshipService {
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly INotificationService _notificationService;
    private readonly ILogger<FriendshipService> _logger;

    public FriendshipService(IDbContextFactory<ApplicationDbContext> DbContextFactory, INotificationService notificationService, ILogger<FriendshipService> logger) {
        _dbContextFactory = DbContextFactory;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<IFriendshipService.SendingStatus> SendFriendRequestAsync(string senderId, string receiverId) {
        if (senderId == receiverId) {
            return IFriendshipService.SendingStatus.Failed;
        }

        await using var database = await _dbContextFactory.CreateDbContextAsync();
        var request = await database.FriendRequests
            .AsNoTracking().FirstOrDefaultAsync(r => (r.SenderId == senderId && r.ReceiverId == receiverId) || (r.SenderId == receiverId && r.ReceiverId == senderId));

        // Determine whether exists a friend request between 2 user ids, order doesn't matter.
        if (request != null) {
            switch (request.Status) {
                case FriendRequestStatus.Canceled or FriendRequestStatus.Rejected or FriendRequestStatus.Unfriended:
                    // There was a friend request, but has been canceled or rejected. Populate it with new data.

                    int numUpdatedRows = await database.FriendRequests
                        .AsNoTracking()
                        .Where(r => (r.SenderId == senderId && r.ReceiverId == receiverId) || (r.SenderId == receiverId && r.ReceiverId == senderId))
                        .ExecuteUpdateAsync(builder => {
                            builder
                                .SetProperty(r => r.SenderId, senderId)
                                .SetProperty(r => r.ReceiverId, receiverId)
                                .SetProperty(r => r.CreatedAt, DateTime.UtcNow)
                                .SetProperty(r => r.ResponseAt, (DateTime?)null)
                                .SetProperty(r => r.Status, FriendRequestStatus.Pending);
                        });

                    switch (numUpdatedRows) {
                        case 0:
                            return IFriendshipService.SendingStatus.Failed;

                        case 1:
                            await _notificationService.NotifyFriendRequestReceivedAsync(new(senderId, receiverId));
                            return IFriendshipService.SendingStatus.Success;

                        default:
                            // I blame concurrency. Still returns Success for the time being.
                            LogUnexpectedNumberOfRowsUpdateWhenRetryFriendRequest(senderId, receiverId, numUpdatedRows);

                            await _notificationService.NotifyFriendRequestReceivedAsync(new(senderId, receiverId));
                            return IFriendshipService.SendingStatus.Success;
                    }

                case FriendRequestStatus.Accepted:
                    return IFriendshipService.SendingStatus.Friended;

                case FriendRequestStatus.Pending:
                    return request.SenderId == senderId ? IFriendshipService.SendingStatus.OutcomingPending : IFriendshipService.SendingStatus.IncomingPending;

                default:
                    throw new UnreachableException("Unknown FriendRequestStatus value.");
            }
        }

        database.FriendRequests.Add(new() {
            SenderId = senderId,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Pending,
        });

        if (await database.SaveChangesAsync() > 0) {
            await _notificationService.NotifyFriendRequestReceivedAsync(new(senderId, receiverId));

            return IFriendshipService.SendingStatus.Success;
        }

        return IFriendshipService.SendingStatus.Failed;
    }

    public async Task<bool> CancelFriendRequestAsync(string senderId, string receiverId) {
        await using var database = await _dbContextFactory.CreateDbContextAsync();
        database.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        int numUpdatedRows = await database.FriendRequests
            .Where(r => r.Status == FriendRequestStatus.Pending && r.SenderId == senderId && r.ReceiverId == receiverId)
            .ExecuteUpdateAsync(builder => {
                builder
                    .SetProperty(r => r.Status, FriendRequestStatus.Canceled)
                    .SetProperty(r => r.ResponseAt, DateTime.UtcNow);
            });

        if (numUpdatedRows > 0) {
            await _notificationService.NotifyFriendRequestCanceledAsync(new(senderId, receiverId));

            return true;
        }

        return false;
    }
    
    public async Task<bool> RejectFriendRequestAsync(string senderId, string receiverId) {
        await using var database = await _dbContextFactory.CreateDbContextAsync();
        database.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        int numUpdatedRows = await database.FriendRequests
            .Where(r => r.Status == FriendRequestStatus.Pending && r.SenderId == senderId && r.ReceiverId == receiverId)
            .ExecuteUpdateAsync(builder => {
                builder
                    .SetProperty(r => r.Status, FriendRequestStatus.Rejected)
                    .SetProperty(r => r.ResponseAt, DateTime.UtcNow);
            });

        if (numUpdatedRows > 0) {
            await _notificationService.NotifyFriendRequestRejectedAsync(new(senderId, receiverId));

            return true;
        }

        return false;
    }
    
    public async Task<bool> AcceptFriendRequestAsync(string senderId, string receiverId) {
        await using var database = await _dbContextFactory.CreateDbContextAsync();
        database.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        int numUpdatedRows = await database.FriendRequests
            .Where(r => r.Status == FriendRequestStatus.Pending && r.SenderId == senderId && r.ReceiverId == receiverId)
            .ExecuteUpdateAsync(builder => {
                builder
                    .SetProperty(r => r.Status, FriendRequestStatus.Accepted)
                    .SetProperty(r => r.ResponseAt, DateTime.UtcNow);
            });

        if (numUpdatedRows > 0) {
            await _notificationService.NotifyFriendRequestAcceptedAsync(new(senderId, receiverId));

            return true;
        }

        return false;
    }

    public async Task<bool> UnfriendAsync(string user1, string user2) {
        await using var database = await _dbContextFactory.CreateDbContextAsync();
        database.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        int numUpdatedRows = await database.FriendRequests
            .Where(r => r.Status == FriendRequestStatus.Accepted && ((r.SenderId == user1 && r.ReceiverId == user2) || (r.SenderId == user2 && r.ReceiverId == user1)))
            .ExecuteUpdateAsync(builder => {
                builder.SetProperty(r => r.Status, FriendRequestStatus.Unfriended);
            });
        
        if (numUpdatedRows > 0) {
            await _notificationService.NotifyUnfriendedAsync(new(user1, user2));

            return true;
        }

        return false;
    }
    
    public async Task<bool> UnfriendAsync(Guid friendRequestId) {
        await using var database = await _dbContextFactory.CreateDbContextAsync();
        database.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        int numUpdatedRows = await database.FriendRequests
            .Where(r => r.Status == FriendRequestStatus.Accepted && r.Id == friendRequestId)
            .ExecuteUpdateAsync(builder => {
                builder.SetProperty(r => r.Status, FriendRequestStatus.Unfriended);
            });
        
        if (numUpdatedRows > 0) {
            var users = await database.FriendRequests
                .Where(x => x.Id == friendRequestId)
                .Select(x => new {
                    x.SenderId,
                    x.ReceiverId
                })
                .FirstOrDefaultAsync();

            if (users != null) {
                await _notificationService.NotifyUnfriendedAsync(new(users.SenderId, users.ReceiverId));

                return true;
            }
        }

        return false;
    }
    
    [LoggerMessage(LogLevel.Error, "Updating FriendRequest between user {from} to {user} cause {numRows} rows to be modified.")]
    private partial void LogUnexpectedNumberOfRowsUpdateWhenRetryFriendRequest(string from, string user, int numRows);
}