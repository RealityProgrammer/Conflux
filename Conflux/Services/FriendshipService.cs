using Conflux.Database;
using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Conflux.Services;

public sealed class FriendshipService : IFriendshipService {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _database;
    private readonly INotificationService _notificationService;

    public FriendshipService(UserManager<ApplicationUser> userManager, ApplicationDbContext database, INotificationService notificationService) {
        _userManager = userManager;
        _database = database;
        _notificationService = notificationService;
    }
    
    public async Task<bool> SendFriendRequest(string senderId, string receiverId) {
        if (senderId == receiverId) return false;

        var request = await _database.FriendRequests.FirstOrDefaultAsync(r => r.SenderId == senderId && r.ReceiverId == receiverId);
        
        if (request != null) {
            if (request.Status == FriendRequestStatus.Rejected) {
                request.CreatedAt = DateTime.UtcNow;
                request.ResponseAt = null;
                request.Status = FriendRequestStatus.Pending;
        
                _database.FriendRequests.Update(request);
                await _database.SaveChangesAsync();
        
                return true;
            }
        
            return false;
        }
        
        await _database.FriendRequests.AddAsync(new() {
            SenderId = senderId,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Pending,
        });
        
        await _database.SaveChangesAsync();
        
        await _notificationService.NotifyFriendRequestAsync(new(senderId, receiverId));
        
        return true;
    }

    public async Task<bool> CancelFriendRequest(string senderId, string receiverId) {
        var request = await _database.FriendRequests
            .FirstOrDefaultAsync(r => r.SenderId == senderId && r.ReceiverId == receiverId);
        
        if (request == null) return false;
        
        _database.FriendRequests.Remove(request);

        await _database.SaveChangesAsync();
        
        return true;
    }

    public Task<FriendRequestStatus?> GetRequestStatus(string senderId, string receiverId) {
        return _database.FriendRequests
            .Where(x => x.SenderId == senderId && x.ReceiverId == receiverId)
            .Select(x => (FriendRequestStatus?)x.Status)
            .FirstOrDefaultAsync();
    }
}