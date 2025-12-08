using Microsoft.AspNetCore.Components;

namespace Conflux.Services.Abstracts;

public readonly record struct FriendRequestReceivedEventArgs(string SenderId, string ReceiverId);
public readonly record struct FriendRequestCanceledEventArgs(string SenderId, string ReceiverId);
public readonly record struct FriendRequestRejectedEventArgs(string SenderId, string ReceiverId);
public readonly record struct FriendRequestAcceptedEventArgs(string SenderId, string ReceiverId);

public interface INotificationService : IAsyncDisposable {
    event Action<FriendRequestReceivedEventArgs>? OnFriendRequestReceived;
    event Action<FriendRequestRejectedEventArgs>? OnFriendRequestRejected;
    event Action<FriendRequestCanceledEventArgs>? OnFriendRequestCanceled;
    event Action<FriendRequestAcceptedEventArgs>? OnFriendRequestAccepted;
    
    Task InitializeConnection(CancellationToken cancellationToken);
    
    Task NotifyFriendRequestReceivedAsync(FriendRequestReceivedEventArgs eventArgs);
    Task NotifyFriendRequestCanceledAsync(FriendRequestCanceledEventArgs eventArgs);
    Task NotifyFriendRequestRejectedAsync(FriendRequestRejectedEventArgs eventArgs);
    Task NotifyFriendRequestAcceptedAsync(FriendRequestAcceptedEventArgs eventArgs);
}