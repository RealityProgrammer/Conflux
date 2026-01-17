using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Conflux.Services.Hubs;

[Authorize]
public sealed class UserNotificationHub : Hub {
    public override async Task OnConnectedAsync() {
        string? userId = Context.GetHttpContext()!.Request.Query["UserId"];

        if (string.IsNullOrEmpty(userId)) {
            throw new ArgumentException("User ID is unspecified.");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception) {
        string? userId = Context.GetHttpContext()!.Request.Query["UserId"];

        if (string.IsNullOrEmpty(userId)) return;

        await base.OnDisconnectedAsync(exception);
    }
}