using Conflux.Database.Entities;

namespace Conflux.Services.Abstracts;

public readonly record struct ChannelCategoryCreatedEventArgs(Guid CommunityId, Guid CategoryId);
public readonly record struct ChannelCreatedEventArgs(Guid ChannelCategoryId, Guid ChannelId);

public interface ICommunityService {
    event Action<ChannelCategoryCreatedEventArgs>? OnChannelCategoryCreated;
    event Action<ChannelCreatedEventArgs>? OnChannelCreated;
    
    Task JoinCommunityHubAsync(Guid communityId);
    Task LeaveCommunityHubAsync(Guid communityId);
    
    Task<bool> CreateServerAsync(string name, Stream? avatarStream, string creatorId);

    Task CreateChannelCategoryAsync(string name, Guid communityId);
    Task CreateChannelAsync(string name, CommunityChannelType type, Guid channelCategoryId);
}