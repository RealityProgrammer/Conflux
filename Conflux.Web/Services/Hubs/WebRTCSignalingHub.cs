using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Conflux.Services.Hubs;

[Authorize]
public sealed class WebRTCSignalingHub : Hub {
    public override async Task OnConnectedAsync() {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception) {
        await base.OnDisconnectedAsync(exception);
    }
}