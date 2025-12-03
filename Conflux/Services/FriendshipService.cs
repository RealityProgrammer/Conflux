using Conflux.Database;
using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Conflux.Services;

public sealed class FriendshipService : IFriendshipService {
    private readonly IDbContextFactory<ApplicationDbContext> _databaseFactory;
    private readonly INotificationService _notificationService;

    public FriendshipService(IDbContextFactory<ApplicationDbContext> databaseFactory, INotificationService notificationService) {
        _databaseFactory = databaseFactory;
        _notificationService = notificationService;
    }

    public async Task<bool> SendFriendRequest(string senderId, string receiverId) {
        if (senderId == receiverId) return false;

        await using (var database = await _databaseFactory.CreateDbContextAsync()) {
            var request = await database.FriendRequests.FirstOrDefaultAsync(r => r.SenderId == senderId && r.ReceiverId == receiverId);

            if (request != null) {
                if (request.Status is FriendRequestStatus.Rejected or FriendRequestStatus.Canceled) {
                    request.CreatedAt = DateTime.UtcNow;
                    request.ResponseAt = null;
                    request.Status = FriendRequestStatus.Pending;
                    
                    await database.SaveChangesAsync();

                    await _notificationService.NotifyFriendRequestReceivedAsync(new(senderId, receiverId));

                    return true;
                }

                return false;
            }

            database.FriendRequests.Add(new() {
                SenderId = senderId,
                ReceiverId = receiverId,
                Status = FriendRequestStatus.Pending,
            });

            await database.SaveChangesAsync();

            await _notificationService.NotifyFriendRequestReceivedAsync(new(senderId, receiverId));
        }

        return true;
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
                .Where(r => r.Status == FriendRequestStatus.Pending && ((r.SenderId == senderId && r.ReceiverId == receiverId) || (r.SenderId == receiverId && r.ReceiverId == senderId)))
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

    public async Task<Pageable<FriendRequest>> GetOutcomingPendingFriendRequests(string userId, PaginationRequest request) {
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

    public async Task<Pageable<FriendRequest>> GetIncomingPendingFriendRequests(string userId, PaginationRequest request) {
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
}