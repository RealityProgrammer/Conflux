namespace Conflux.Domain.Events;

public readonly record struct UnfriendedEventArgs(Guid RequestId, string User1, string User2);
