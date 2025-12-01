using Conflux.Services.Abstracts;
using Conflux.Services.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Conflux.Services;

public class NotificationService(
    ILogger<NotificationService> logger,
    IHubContext<NotificationHub> hubContext
) : INotificationService {
    
    public async Task NotifyFriendRequestAsync(NotifyFriendRequestModel model) {
        var user = hubContext.Clients.User(model.ReceiverId);
        
        await user.SendAsync("ReceiveFriendRequest", model.SenderId);
    }
}