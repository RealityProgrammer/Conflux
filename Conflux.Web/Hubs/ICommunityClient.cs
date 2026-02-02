using Conflux.Domain.Events;

namespace Conflux.Web.Hubs;

public interface ICommunityClient {
    Task ChannelCategoryCreated(ChannelCategoryCreatedEventArgs args);
    Task ChannelCreated(ChannelCreatedEventArgs args);
    Task MemberJoined(CommunityMemberJoinedEventArgs args);
    Task RoleCreated(CommunityRoleCreatedEventArgs args);
    Task RoleRenamed(CommunityRoleRenamedEventArgs args);
    Task RoleDeleted(CommunityRoleDeletedEventArgs args);
    Task RolePermissionUpdated(CommunityRolePermissionUpdatedEventArg args);
    Task MemberRoleChanged(MemberRoleChangedEventArgs args);
}