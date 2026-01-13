using Conflux.Application.Dto;
using Conflux.Domain.Entities;

namespace Conflux.Application.Abstracts;

public readonly record struct CommunityCreatedEventArgs(Community Community);

public interface ICommunityService {
    event Action<CommunityCreatedEventArgs>? OnUserCreatedCommunity;
    
    Task<bool> CreateCommunityAsync(string name, Stream? avatarStream, string creatorId);

    Task CreateChannelCategoryAsync(string name, Guid communityId);
    Task CreateChannelAsync(string name, CommunityChannelType type, Guid channelCategoryId);

    Task<CreateRoleStatus> CreateRoleAsync(Guid communityId, string roleName);

    Task<bool> JoinCommunityAsync(string userId, Guid communityId, Guid invitationId);
    
    Task<RolePermissions?> GetPermissionsAsync(Guid roleId);
    Task<bool> UpdatePermissionsAsync(Guid communityId, Guid roleId, RolePermissions permissions);

     Task<RolePermissionsWithId?> GetUserRolePermissionsAsync(string userId, Guid communityId);

    Task<bool> SetMembersRole(Guid communityId, IReadOnlyCollection<Guid> memberIds, Guid? roleId);

    Task<bool> RenameRole(Guid communityId, Guid roleId, string name);

    Task<bool> DeleteRole(Guid communityId, Guid roleId);

    public enum CreateRoleStatus {
        Success,
        Failure,
        NameExists,
        ReservedName,
    }
}