namespace Conflux.Domain.Events;

public readonly record struct CommunityMemberJoinedEventArgs(Guid CommunityId, string UserId);