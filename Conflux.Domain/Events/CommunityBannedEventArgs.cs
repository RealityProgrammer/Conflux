namespace Conflux.Domain.Events;

public readonly record struct CommunityBannedEventArgs(Guid CommunityId, Guid MemberId, Guid UserId);