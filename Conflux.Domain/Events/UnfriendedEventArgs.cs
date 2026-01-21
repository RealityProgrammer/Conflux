namespace Conflux.Domain.Events;

public readonly record struct UnfriendedEventArgs(Guid RequestId, Guid User1, Guid User2);
