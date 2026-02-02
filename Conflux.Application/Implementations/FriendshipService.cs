using Conflux.Application.Abstracts;
using Conflux.Domain;
using Conflux.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Conflux.Application.Implementations;

public sealed partial class FriendshipService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IFriendshipEventDispatcher eventDispatcher,
    ILogger<FriendshipService> logger
) : IFriendshipService {
    public async Task<IFriendshipService.SendingResult> SendFriendRequestAsync(Guid senderId, Guid receiverId) {
        if (senderId == receiverId) {
            return new(IFriendshipService.SendingStatus.Failed, null);
        }

        await using var database = await dbContextFactory.CreateDbContextAsync();
        var requestData = await database.FriendRequests
            .AsNoTracking()
            .Where(r => (r.SenderId == senderId && r.ReceiverId == receiverId) || (r.SenderId == receiverId && r.ReceiverId == senderId))
            .Select(x => new { x.Id, x.Status, x.SenderId })
            .FirstOrDefaultAsync();

        // Determine whether exists a friend request between 2 user ids, order doesn't matter.
        if (requestData != null) {
            switch (requestData.Status) {
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
                            return new(IFriendshipService.SendingStatus.Failed, null);

                        case 1:
                            await eventDispatcher.NotifyFriendRequestReceivedAsync(new(requestData.Id, senderId, receiverId));
                            return new(IFriendshipService.SendingStatus.Success, requestData.Id);

                        default:
                            // I blame concurrency. Still returns Success for the time being.
                            LogUnexpectedNumberOfRowsUpdateWhenRetryFriendRequest(senderId, receiverId, numUpdatedRows);

                            await eventDispatcher.NotifyFriendRequestReceivedAsync(new(requestData.Id, senderId, receiverId));
                            return new(IFriendshipService.SendingStatus.Success, requestData.Id);
                    }

                case FriendRequestStatus.Accepted:
                    return new(IFriendshipService.SendingStatus.Friended, requestData.Id);

                case FriendRequestStatus.Pending:
                    return new(requestData.SenderId == senderId ? IFriendshipService.SendingStatus.OutcomingPending : IFriendshipService.SendingStatus.IncomingPending, requestData.Id);

                default:
                    throw new UnreachableException("Unknown FriendRequestStatus value.");
            }
        }

        FriendRequest newRequest = new() {
            SenderId = senderId,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        database.FriendRequests.Add(newRequest);

        if (await database.SaveChangesAsync() > 0) {
            await eventDispatcher.NotifyFriendRequestReceivedAsync(new(newRequest.Id, senderId, receiverId));

            return new(IFriendshipService.SendingStatus.Success, newRequest.Id);
        }

        return new(IFriendshipService.SendingStatus.Failed, null);
    }

    public async Task<bool> CancelFriendRequestAsync(Guid friendRequestId) {
        await using var database = await dbContextFactory.CreateDbContextAsync();
        database.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var receiverId = await database.FriendRequests
            .Where(r => r.Status == FriendRequestStatus.Pending && r.Id == friendRequestId)
            .Select(r => r.ReceiverId)
            .FirstOrDefaultAsync();

        if (receiverId == Guid.Empty) {
            return false;
        }
        
        int numUpdatedRows = await database.FriendRequests
            .Where(r => r.Status == FriendRequestStatus.Pending && r.Id == friendRequestId)
            .ExecuteUpdateAsync(builder => {
                builder
                    .SetProperty(r => r.Status, FriendRequestStatus.Canceled)
                    .SetProperty(r => r.ResponseAt, DateTime.UtcNow);
            });

        if (numUpdatedRows > 0) {
            await eventDispatcher.NotifyFriendRequestCanceledAsync(new(friendRequestId, receiverId));

            return true;
        }

        return false;
    }
    
    public async Task<bool> RejectFriendRequestAsync(Guid friendRequestId) {
        await using var database = await dbContextFactory.CreateDbContextAsync();
        database.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        var senderId = await database.FriendRequests
            .Where(r => r.Status == FriendRequestStatus.Pending && r.Id == friendRequestId)
            .Select(r => r.SenderId)
            .FirstOrDefaultAsync();

        if (senderId == Guid.Empty) {
            return false;
        }
        
        int numUpdatedRows = await database.FriendRequests
            .Where(r => r.Status == FriendRequestStatus.Pending && r.Id == friendRequestId)
            .ExecuteUpdateAsync(builder => {
                builder
                    .SetProperty(r => r.Status, FriendRequestStatus.Rejected)
                    .SetProperty(r => r.ResponseAt, DateTime.UtcNow);
            });

        if (numUpdatedRows > 0) {
            await eventDispatcher.NotifyFriendRequestRejectedAsync(new(friendRequestId, senderId));

            return true;
        }

        return false;
    }
    
    public async Task<bool> AcceptFriendRequestAsync(Guid friendRequestId) {
        await using var database = await dbContextFactory.CreateDbContextAsync();
        database.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        var senderId = await database.FriendRequests
            .Where(r => r.Status == FriendRequestStatus.Pending && r.Id == friendRequestId)
            .Select(r => r.SenderId)
            .FirstOrDefaultAsync();

        if (senderId == Guid.Empty) {
            return false;
        }
        
        int numUpdatedRows = await database.FriendRequests
            .Where(r => r.Status == FriendRequestStatus.Pending && r.Id == friendRequestId)
            .ExecuteUpdateAsync(builder => {
                builder
                    .SetProperty(r => r.Status, FriendRequestStatus.Accepted)
                    .SetProperty(r => r.ResponseAt, DateTime.UtcNow);
            });

        if (numUpdatedRows > 0) {
            await eventDispatcher.NotifyFriendRequestAcceptedAsync(new(friendRequestId, senderId));

            return true;
        }

        return false;
    }

    public async Task<bool> UnfriendAsync(Guid friendRequestId) {
        await using var database = await dbContextFactory.CreateDbContextAsync();
        database.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        var userIds = await database.FriendRequests
            .Where(r => r.Status == FriendRequestStatus.Accepted && r.Id == friendRequestId)
            .Select(r => new { r.SenderId, r.ReceiverId })
            .FirstOrDefaultAsync();

        if (userIds == null) {
            return false;
        }
        
        int numUpdatedRows = await database.FriendRequests
            .Where(r => r.Status == FriendRequestStatus.Accepted && r.Id == friendRequestId)
            .ExecuteUpdateAsync(builder => {
                builder.SetProperty(r => r.Status, FriendRequestStatus.Unfriended);
                builder.SetProperty(r => r.ResponseAt, (DateTime?)null);
            });
        
        if (numUpdatedRows > 0) {
            await eventDispatcher.NotifyUnfriendedAsync(new(friendRequestId, userIds.SenderId, userIds.ReceiverId));

            return true;
        }

        return false;
    }
    
    [LoggerMessage(LogLevel.Error, "Updating FriendRequest between user {from} to {user} cause {numRows} rows to be modified.")]
    private partial void LogUnexpectedNumberOfRowsUpdateWhenRetryFriendRequest(Guid from, Guid user, int numRows);
}