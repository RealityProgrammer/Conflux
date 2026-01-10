namespace Conflux.Domain.Events;

public readonly record struct FriendRequestReceivedEventArgs(Guid RequestId, string SenderId, string ReceiverId);
public readonly record struct FriendRequestCanceledEventArgs(Guid RequestId, string ReceiverId);
public readonly record struct FriendRequestRejectedEventArgs(Guid RequestId, string SenderId);
public readonly record struct FriendRequestAcceptedEventArgs(Guid RequestId, string SenderId);
public readonly record struct UnfriendedEventArgs(Guid RequestId, string User1, string User2);

public interface IFriendshipEventDispatcher : IAsyncDisposable {
    event Action<FriendRequestReceivedEventArgs>? OnFriendRequestReceived;
    event Action<FriendRequestRejectedEventArgs>? OnFriendRequestRejected;
    event Action<FriendRequestCanceledEventArgs>? OnFriendRequestCanceled;
    event Action<FriendRequestAcceptedEventArgs>? OnFriendRequestAccepted;
    event Action<UnfriendedEventArgs>? OnUnfriended;
    
    Task Connect();
    Task Disconnect();
    
    Task NotifyFriendRequestReceivedAsync(FriendRequestReceivedEventArgs args);
    Task NotifyFriendRequestCanceledAsync(FriendRequestCanceledEventArgs args);
    Task NotifyFriendRequestRejectedAsync(FriendRequestRejectedEventArgs args);
    Task NotifyFriendRequestAcceptedAsync(FriendRequestAcceptedEventArgs args);
    Task NotifyUnfriendedAsync(UnfriendedEventArgs args);
}