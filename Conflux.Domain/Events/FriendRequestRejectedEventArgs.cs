namespace Conflux.Domain.Events;

public readonly record struct FriendRequestRejectedEventArgs(Guid RequestId, string SenderId);
