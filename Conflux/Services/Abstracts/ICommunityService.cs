using Conflux.Database.Entities;

namespace Conflux.Services.Abstracts;

public readonly record struct CommunityCreatedEventArgs(Community Community);
public readonly record struct ChannelCategoryCreatedEventArgs(Guid CategoryId);
public readonly record struct ChannelCreatedEventArgs(Guid ChannelCategoryId, Guid ChannelId);
public readonly record struct CommunityMemberJoinedEventArgs(CommunityMember Member);
public readonly record struct CommunityRoleCreatedEventArgs(CommunityRole Role);
public readonly record struct CommunityRoleRenamedEventArgs(Guid RoleId, string Name);
public readonly record struct CommunityRoleDeletedEventArgs(Guid RoleId);
public readonly record struct CommunityRolePermissionUpdatedEventArg(Guid RoleId);
public readonly record struct MemberRoleChangedEventArgs(Guid? RoleId);

public interface ICommunityService {
    event Action<ChannelCategoryCreatedEventArgs>? OnChannelCategoryCreated;
    event Action<ChannelCreatedEventArgs>? OnChannelCreated;
    event Action<CommunityCreatedEventArgs>? OnUserCreatedCommunity;
    event Action<CommunityMemberJoinedEventArgs>? OnMemberJoined;
    event Action<CommunityRoleCreatedEventArgs>? OnRoleCreated;
    event Action<CommunityRoleRenamedEventArgs>? OnRoleRenamed;
    event Action<CommunityRoleDeletedEventArgs>? OnRoleDeleted;
    event Action<CommunityRolePermissionUpdatedEventArg>? OnRolePermissionUpdated;
    event Action<MemberRoleChangedEventArgs>? OnMemberRoleChanged;
    
    Task JoinCommunityHubAsync(Guid communityId);
    Task LeaveCommunityHubAsync(Guid communityId);
    
    Task<bool> CreateCommunityAsync(string name, Stream? avatarStream, string creatorId);

    Task CreateChannelCategoryAsync(string name, Guid communityId);
    Task CreateChannelAsync(string name, CommunityChannelType type, Guid channelCategoryId);

    Task<CreateRoleStatus> CreateRoleAsync(Guid communityId, string roleName);

    Task<bool> JoinCommunityAsync(string userId, Guid communityId, Guid invitationId);
    
    Task<Permissions?> GetPermissionsAsync(Guid roleId);
    Task<bool> UpdatePermissionsAsync(Guid communityId, Guid roleId, Permissions permissions);

    Task<Guid?> GetUserRoleAsync(Guid communityId, string userId);
    Task<Permissions?> GetUserRolePermissionsAsync(string userId, Guid communityId);
    Task<(Guid?, Permissions)?> GetUserRoleInformationAsync(Guid communityId, string userId);

    Task<bool> SetMembersRole(Guid communityId, IReadOnlyCollection<Guid> memberIds, Guid? roleId);

    Task<bool> RenameRole(Guid communityId, Guid roleId, string name);

    Task<bool> DeleteRole(Guid communityId, Guid roleId);

    public enum CreateRoleStatus {
        Success,
        Failure,
        NameExists,
        ReservedName,
    }
    
    public record Permissions(
        CommunityRole.ChannelPermissionFlags ChannelPermissions,
        CommunityRole.RolePermissionFlags RolePermissions,
        CommunityRole.AccessPermissionFlags AccessPermissions
    );
}