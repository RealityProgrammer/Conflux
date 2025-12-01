using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace Conflux.Services.Hubs;

public sealed class NotificationHub : Hub {
    // Hubs are transient.
    private static readonly ConcurrentDictionary<string, string> _userConnections = [];
    
    public override Task OnConnectedAsync() {
        var userId = Context.UserIdentifier;
        
        if (!string.IsNullOrEmpty(userId))
        {
            _userConnections[userId] = Context.ConnectionId;
        }
        
        return base.OnConnectedAsync();
    }

    public async Task SendFriendRequestNotification(string userId, string receiverId) {
        await Clients.User(receiverId).SendAsync("ReceiveFriendRequest", userId);
    }

    public override Task OnDisconnectedAsync(Exception? exception) {
        var userId = Context.UserIdentifier;

        if (!string.IsNullOrEmpty(userId)) {
            _userConnections.Remove(userId, out _);
        }

        return base.OnDisconnectedAsync(exception);
    }
}