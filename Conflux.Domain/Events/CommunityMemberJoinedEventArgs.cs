namespace Conflux.Domain.Events;

public readonly record struct CommunityMemberJoinedEventArgs(Guid CommunityId, Guid UserId);