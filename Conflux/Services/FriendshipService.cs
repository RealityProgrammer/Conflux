using Conflux.Database;
using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Conflux.Services;

public sealed class FriendshipService(UserManager<ApplicationUser> userManager, ApplicationDbContext database) : IFriendshipService {
    public async Task<bool> SendFriendRequest(string senderId, string receiverId) {
        if (senderId == receiverId) return false;

        var existingRequest = await database.FriendRequests.FirstOrDefaultAsync(
            r => r.SenderId == senderId && r.ReceiverId == receiverId
        );

        if (existingRequest != null) {
            if (existingRequest.Status == FriendRequestStatus.Rejected) {
                existingRequest.CreatedAt = DateTime.UtcNow;
                existingRequest.ResponseAt = null;
                existingRequest.Status = FriendRequestStatus.Pending;

                await database.SaveChangesAsync();

                return true;
            }

            return false;
        }

        await database.FriendRequests.AddAsync(new() {
            SenderId = senderId,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Pending,
        });

        await database.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CancelFriendRequest(string senderId, string receiverId) {
        int numDeletedRows = await database.FriendRequests
            .Where(r => r.SenderId == senderId && r.ReceiverId == receiverId)
            .ExecuteDeleteAsync();

        return numDeletedRows > 0;
    }

    public Task<FriendRequestStatus?> GetRequestStatus(string senderId, string receiverId) {
        return database.FriendRequests
            .Where(x => x.SenderId == senderId && x.ReceiverId == receiverId)
            .Select(x => (FriendRequestStatus?)x.Status)
            .FirstOrDefaultAsync();
    }
}