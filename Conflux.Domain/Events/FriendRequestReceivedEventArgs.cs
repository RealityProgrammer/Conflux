namespace Conflux.Domain.Events;

public readonly record struct FriendRequestReceivedEventArgs(Guid RequestId, Guid SenderId, Guid ReceiverId);
