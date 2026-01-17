namespace Conflux.Domain.Events;

public readonly record struct FriendRequestReceivedEventArgs(Guid RequestId, string SenderId, string ReceiverId);
