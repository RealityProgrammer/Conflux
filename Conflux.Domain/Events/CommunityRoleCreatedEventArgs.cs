namespace Conflux.Domain.Events;

public readonly record struct CommunityRoleCreatedEventArgs(Guid CommunityId, Guid RoleId, string RoleName);
