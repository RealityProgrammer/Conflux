using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Conflux.Services.Hubs;

[Authorize]
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

    public override Task OnDisconnectedAsync(Exception? exception) {
        var userId = Context.UserIdentifier;

        if (!string.IsNullOrEmpty(userId)) {
            _userConnections.Remove(userId, out _);
        }

        return base.OnDisconnectedAsync(exception);
    }
}