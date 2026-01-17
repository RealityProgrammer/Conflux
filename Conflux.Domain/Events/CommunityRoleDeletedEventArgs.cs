namespace Conflux.Domain.Events;

public readonly record struct CommunityRoleDeletedEventArgs(Guid CommunityId, Guid RoleId);
