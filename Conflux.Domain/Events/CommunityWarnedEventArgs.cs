namespace Conflux.Domain.Events;

public readonly record struct CommunityWarnedEventArgs(Guid CommunityId, Guid MemberId, Guid UserId);