using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Conflux.Web.Hubs;

[Authorize]
public sealed class CommunityHub : Hub<ICommunityClient> {
    public override async Task OnConnectedAsync() {
        string? community = Context.GetHttpContext()!.Request.Query["CommunityId"];

        if (string.IsNullOrEmpty(community)) {
            throw new ArgumentException("Community ID is unspecified.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, community);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception) {
        string? community = Context.GetHttpContext()!.Request.Query["CommunityId"];

        if (string.IsNullOrEmpty(community)) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, community);
        await base.OnDisconnectedAsync(exception);
    }
}