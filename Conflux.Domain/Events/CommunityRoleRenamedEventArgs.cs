namespace Conflux.Domain.Events;

public readonly record struct CommunityRoleRenamedEventArgs(Guid CommunityId, Guid RoleId, string NewName);
