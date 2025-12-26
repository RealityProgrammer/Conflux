using Conflux.Database.Entities;

namespace Conflux.Services.Abstracts;

public readonly record struct CommunityCreatedEventArgs(Community Community);
public readonly record struct ChannelCategoryCreatedEventArgs(Guid CommunityId, Guid CategoryId);
public readonly record struct ChannelCreatedEventArgs(Guid ChannelCategoryId, Guid ChannelId);
public readonly record struct CommunityMemberJoinedEventArgs(CommunityMember Member);
public readonly record struct CommunityRoleCreatedEventArgs(CommunityRole Role);

public interface ICommunityService {
    event Action<ChannelCategoryCreatedEventArgs>? OnChannelCategoryCreated;
    event Action<ChannelCreatedEventArgs>? OnChannelCreated;
    event Action<CommunityCreatedEventArgs>? OnUserCreatedCommunity;
    event Action<CommunityMemberJoinedEventArgs>? OnMemberJoined;
    event Action<CommunityRoleCreatedEventArgs>? OnRoleCreated;
    
    Task JoinCommunityHubAsync(Guid communityId);
    Task LeaveCommunityHubAsync(Guid communityId);
    
    Task<bool> CreateCommunityAsync(string name, Stream? avatarStream, string creatorId);

    Task CreateChannelCategoryAsync(string name, Guid communityId);
    Task CreateChannelAsync(string name, CommunityChannelType type, Guid channelCategoryId);

    Task<CreateRoleStatus> CreateRoleAsync(Guid communityId, string roleName);

    Task<bool> JoinCommunityAsync(string userId, Guid communityId, Guid invitationId);

    public enum CreateRoleStatus {
        Success,
        Failure,
        NameExists,
        ReservedName,
    }
}