namespace Conflux.Domain.Events;

public readonly record struct MemberRoleChangedEventArgs(Guid CommunityId, Guid? RoleId);
