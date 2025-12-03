using Conflux.Database;
using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Conflux.Services;

public sealed partial class FriendshipService : IFriendshipService {
    private readonly IDbContextFactory<ApplicationDbContext> _databaseFactory;
    private readonly INotificationService _notificationService;
    private readonly ILogger<FriendshipService> _logger;

    public FriendshipService(IDbContextFactory<ApplicationDbContext> databaseFactory, INotificationService notificationService, ILogger<FriendshipService> logger) {
        _databaseFactory = databaseFactory;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<IFriendshipService.SendingStatus> SendFriendRequest(string senderId, string receiverId) {
        if (senderId == receiverId) {
            return IFriendshipService.SendingStatus.Failed;
        }

        await using (var database = await _databaseFactory.CreateDbContextAsync()) {
            var request = await database.FriendRequests
                .AsNoTracking().FirstOrDefaultAsync(r => (r.SenderId == senderId && r.ReceiverId == receiverId) || (r.SenderId == receiverId && r.ReceiverId == senderId));

            // Determine whether exists a friend request between 2 user ids, order doesn't matter.
            if (request != null) {
                switch (request.Status) {
                    case FriendRequestStatus.Canceled or FriendRequestStatus.Rejected:
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

            await database.SaveChangesAsync();

            await _notificationService.NotifyFriendRequestReceivedAsync(new(senderId, receiverId));

            return IFriendshipService.SendingStatus.Success;
        }
    }

    public async Task<bool> CancelFriendRequest(string senderId, string receiverId) {
        await using (var database = await _databaseFactory.CreateDbContextAsync()) {
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
    }
    
    public async Task<bool> RejectFriendRequest(string senderId, string receiverId) {
        await using (var database = await _databaseFactory.CreateDbContextAsync()) {
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
    }
    
    public async Task<bool> AcceptFriendRequest(string senderId, string receiverId) {
        await using (var database = await _databaseFactory.CreateDbContextAsync()) {
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
    }

    public async Task<Pageable<FriendRequest>> GetOutcomingPendingFriendRequests(string userId, QueryRequest request) {
        await using (var database = await _databaseFactory.CreateDbContextAsync()) {
            database.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            IQueryable<FriendRequest> query = database.Users
                .Where(u => u.Id == userId)
                .SelectMany(x => x.SentFriendRequests)
                .Where(x => x.Status == FriendRequestStatus.Pending)
                .Include(r => r.Receiver);

            if (request.Includes is { } includes) {
                query = includes(query);
            }
            
            if (request.Filter is { } filter) {
                query = filter(query);
            }

            int totalCount = await query.CountAsync();

            if (totalCount == 0) {
                return new(0, 0, []);
            }

            var requests = await request.Order(query)
                .Skip(request.Offset)
                .Take(request.Count)
                .ToListAsync();

            return new(totalCount, request.Offset, requests);
        }
    }

    public async Task<Pageable<FriendRequest>> GetIncomingPendingFriendRequests(string userId, QueryRequest request) {
        await using (var database = await _databaseFactory.CreateDbContextAsync()) {
            database.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            IQueryable<FriendRequest> query = database.Users
                .Where(u => u.Id == userId)
                .SelectMany(x => x.ReceivedFriendRequests)
                .Where(x => x.Status == FriendRequestStatus.Pending)
                .Include(r => r.Sender);

            if (request.Includes is { } includes) {
                query = includes(query);
            }
            
            if (request.Filter is { } filter) {
                query = filter(query);
            }

            int totalCount = await query.CountAsync();

            if (totalCount == 0) {
                return new(0, 0, []);
            }

            var requests = await request.Order(query)
                .Skip(request.Offset)
                .Take(request.Count)
                .ToListAsync();

            return new(totalCount, request.Offset, requests);
        }
    }
    
    [LoggerMessage(LogLevel.Error, "Updating FriendRequest between user {from} to {user} cause {numRows} rows to be modified.")]
    private partial void LogUnexpectedNumberOfRowsUpdateWhenRetryFriendRequest(string from, string user, int numRows);
}