using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Conflux.Web.Hubs;

[Authorize]
public sealed class CallingHub : Hub<ICallClient> {
    public override async Task OnConnectedAsync() {
        string? callId = Context.GetHttpContext()!.Request.Query["CallId"];

        if (string.IsNullOrEmpty(callId) || !Guid.TryParse(callId, out _)) {
            throw new ArgumentException("CallID is required.");
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, callId);
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception) {
        string? community = Context.GetHttpContext()!.Request.Query["CallId"];

        if (string.IsNullOrEmpty(community)) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, community);
        await base.OnDisconnectedAsync(exception);
    }
}