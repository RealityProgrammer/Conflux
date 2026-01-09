using Microsoft.AspNetCore.Components;

namespace Conflux.Services.Abstracts;

public readonly record struct FriendRequestReceivedEventArgs(string SenderId, string ReceiverId);
public readonly record struct FriendRequestCanceledEventArgs(string SenderId, string ReceiverId);
public readonly record struct FriendRequestRejectedEventArgs(string SenderId, string ReceiverId);
public readonly record struct FriendRequestAcceptedEventArgs(string SenderId, string ReceiverId);
public readonly record struct UnfriendedEventArgs(string User1, string User2);

public interface INotificationService : IAsyncDisposable {
    event Action<FriendRequestReceivedEventArgs>? OnFriendRequestReceived;
    event Action<FriendRequestRejectedEventArgs>? OnFriendRequestRejected;
    event Action<FriendRequestCanceledEventArgs>? OnFriendRequestCanceled;
    event Action<FriendRequestAcceptedEventArgs>? OnFriendRequestAccepted;
    event Action<UnfriendedEventArgs>? OnUnfriended;
    
    Task JoinNotificationHub(CancellationToken cancellationToken);
    Task LeaveNotificationHub(CancellationToken cancellationToken);
    
    Task NotifyFriendRequestReceivedAsync(FriendRequestReceivedEventArgs args);
    Task NotifyFriendRequestCanceledAsync(FriendRequestCanceledEventArgs args);
    Task NotifyFriendRequestRejectedAsync(FriendRequestRejectedEventArgs args);
    Task NotifyFriendRequestAcceptedAsync(FriendRequestAcceptedEventArgs args);
    Task NotifyUnfriendedAsync(UnfriendedEventArgs args);
}