using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Conflux.Services.Hubs;

[Authorize]
public sealed class ConversationHub : Hub {
    public override async Task OnConnectedAsync() {
        string? conversationId = Context.GetHttpContext()!.Request.Query["ConversationId"];

        if (string.IsNullOrEmpty(conversationId)) {
            throw new ArgumentException("Conversation ID is unspecified.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception) {
        string? conversationId = Context.GetHttpContext()!.Request.Query["ConversationId"];

        if (string.IsNullOrEmpty(conversationId)) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
        await base.OnDisconnectedAsync(exception);
    }
}