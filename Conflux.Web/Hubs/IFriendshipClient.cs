using Conflux.Domain.Events;

namespace Conflux.Web.Hubs;

public interface IFriendshipClient {
   Task FriendRequestReceived(FriendRequestReceivedEventArgs args);
   Task FriendRequestRejected(FriendRequestRejectedEventArgs args);
   Task FriendRequestCanceled(FriendRequestCanceledEventArgs args);
   Task FriendRequestAccepted(FriendRequestAcceptedEventArgs args);
   Task Unfriended(UnfriendedEventArgs args);
}