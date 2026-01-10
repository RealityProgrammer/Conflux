using Conflux.Domain.Entities;

namespace Conflux.Domain.Events;

public readonly record struct ChannelCategoryCreatedEventArgs(Guid CommunityId, Guid CategoryId, string CategoryName);
public readonly record struct ChannelCreatedEventArgs(Guid CommunityId, Guid CategoryId, Guid ChannelId, string ChannelName, CommunityChannelType ChannelType);
public readonly record struct CommunityMemberJoinedEventArgs(Guid CommunityId, string UserId);
public readonly record struct CommunityRoleCreatedEventArgs(Guid CommunityId, Guid RoleId, string RoleName);
public readonly record struct CommunityRoleRenamedEventArgs(Guid CommunityId, Guid RoleId, string NewName);
public readonly record struct CommunityRoleDeletedEventArgs(Guid CommunityId, Guid RoleId);
public readonly record struct CommunityRolePermissionUpdatedEventArg(Guid CommunityId, Guid RoleId);
public readonly record struct MemberRoleChangedEventArgs(Guid CommunityId, Guid? RoleId);

public interface ICommunityEventDispatcher {
    event Action<ChannelCategoryCreatedEventArgs>? OnChannelCategoryCreated;
    event Action<ChannelCreatedEventArgs>? OnChannelCreated;
    event Action<CommunityMemberJoinedEventArgs>? OnMemberJoined;
    event Action<CommunityRoleCreatedEventArgs>? OnRoleCreated;
    event Action<CommunityRoleRenamedEventArgs>? OnRoleRenamed;
    event Action<CommunityRoleDeletedEventArgs>? OnRoleDeleted;
    event Action<CommunityRolePermissionUpdatedEventArg>? OnRolePermissionUpdated;
    event Action<MemberRoleChangedEventArgs>? OnMemberRoleChanged;

    Task Connect(Guid communityId);
    Task Disconnect(Guid communityId);

    Task Dispatch(ChannelCategoryCreatedEventArgs args);
    Task Dispatch(ChannelCreatedEventArgs args);
    Task Dispatch(CommunityMemberJoinedEventArgs args);
    Task Dispatch(CommunityRoleCreatedEventArgs args);
    Task Dispatch(CommunityRoleRenamedEventArgs args);
    Task Dispatch(CommunityRoleDeletedEventArgs args);
    Task Dispatch(CommunityRolePermissionUpdatedEventArg args);
    Task Dispatch(MemberRoleChangedEventArgs args);
}