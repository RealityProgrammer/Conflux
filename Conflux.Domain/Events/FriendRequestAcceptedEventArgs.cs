namespace Conflux.Domain.Events;

public readonly record struct FriendRequestAcceptedEventArgs(Guid RequestId, Guid SenderId);
