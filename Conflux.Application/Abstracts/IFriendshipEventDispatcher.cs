using Conflux.Domain.Events;

namespace Conflux.Application.Abstracts;

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