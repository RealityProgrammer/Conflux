using Conflux.Database;
using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Conflux.Services;

public sealed class FriendshipService : IFriendshipService {
    private readonly ApplicationDbContext _database;
    private readonly INotificationService _notificationService;

    public FriendshipService(ApplicationDbContext database, INotificationService notificationService) {
        _database = database;
        _notificationService = notificationService;
    }
    
    public async Task<bool> SendFriendRequest(string senderId, string receiverId) {
        if (senderId == receiverId) return false;

        var request = await _database.FriendRequests.AsNoTracking().FirstOrDefaultAsync(r => r.SenderId == senderId && r.ReceiverId == receiverId);

        if (request != null) {
            if (request.Status is FriendRequestStatus.Rejected or FriendRequestStatus.Canceled) {
                request.CreatedAt = DateTime.UtcNow;
                request.ResponseAt = null;
                request.Status = FriendRequestStatus.Pending;
                
                _database.FriendRequests.Update(request);
                await _database.SaveChangesAsync();
                
                await _notificationService.NotifyFriendRequestReceivedAsync(new(senderId, receiverId));

                return true;
            }

            return false;
        }
        
        _database.FriendRequests.Add(new() {
            SenderId = senderId,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Pending,
        });
        
        await _database.SaveChangesAsync();
        
        await _notificationService.NotifyFriendRequestReceivedAsync(new(senderId, receiverId));
        
        return true;
    }

    public async Task<bool> CancelFriendRequest(string senderId, string receiverId) {
        var request = await _database.FriendRequests
            .Where(x => x.Status == FriendRequestStatus.Pending)
            .FirstOrDefaultAsync(r => r.SenderId == senderId && r.ReceiverId == receiverId);

        if (request == null) {
            return false;
        }
        
        request.Status = FriendRequestStatus.Canceled;
        request.ResponseAt = DateTime.UtcNow;

        await _database.SaveChangesAsync();

        await _notificationService.NotifyFriendRequestCanceledAsync(new(senderId, receiverId));
        
        return true;
    }
    
    public async Task<bool> RejectFriendRequest(string senderId, string receiverId) {
        var request = await _database.FriendRequests
            .Where(x => x.Status == FriendRequestStatus.Pending)
            .FirstOrDefaultAsync(r => r.SenderId == senderId && r.ReceiverId == receiverId);

        if (request == null) {
            return false;
        }
        
        request.Status = FriendRequestStatus.Rejected;
        request.ResponseAt = DateTime.UtcNow;
        
        await _database.SaveChangesAsync();
        
        await _notificationService.NotifyFriendRequestRejectedAsync(new(senderId, receiverId));
        
        return true;
    }

    public Task<FriendRequestStatus?> GetRequestStatus(string senderId, string receiverId) {
        return _database.FriendRequests
            .Where(x => x.SenderId == senderId && x.ReceiverId == receiverId)
            .Select(x => (FriendRequestStatus?)x.Status)
            .FirstOrDefaultAsync();
    }
}