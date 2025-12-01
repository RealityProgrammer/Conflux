using Microsoft.AspNetCore.Components;

namespace Conflux.Services.Abstracts;

public record FriendRequestReceivedNotification(string SenderId, string ReceiverId);
public record FriendRequestCanceledNotification(string SenderId, string ReceiverId);
public record FriendRequestRejectedNotification(string SenderId, string ReceiverId);

public interface INotificationService : IAsyncDisposable {
    event Action<FriendRequestReceivedNotification>? OnFriendRequestReceived;
    event Action<FriendRequestRejectedNotification>? OnFriendRequestRejected;
    event Action<FriendRequestCanceledNotification>? OnFriendRequestCanceled;
    
    Task InitializeConnection(CancellationToken cancellationToken);
    
    Task NotifyFriendRequestReceivedAsync(FriendRequestReceivedNotification notification);
    Task NotifyFriendRequestCanceledAsync(FriendRequestCanceledNotification notification);
    Task NotifyFriendRequestRejectedAsync(FriendRequestRejectedNotification notification);
}