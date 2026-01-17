namespace Conflux.Domain.Events;

public readonly record struct CommunityRolePermissionUpdatedEventArg(Guid CommunityId, Guid RoleId);
