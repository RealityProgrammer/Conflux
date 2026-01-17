using Conflux.Domain.Events;

namespace Conflux.Application.Abstracts;

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