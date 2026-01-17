namespace Conflux.Domain.Events;

public readonly record struct FriendRequestCanceledEventArgs(Guid RequestId, string ReceiverId);
