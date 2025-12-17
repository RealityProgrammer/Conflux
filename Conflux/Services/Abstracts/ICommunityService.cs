namespace Conflux.Services.Abstracts;

public readonly record struct ChannelCategoryCreatedEventArgs(Guid CommunityId, Guid CategoryId);

public interface ICommunityService {
    event Action<ChannelCategoryCreatedEventArgs>? OnChannelCategoryCreated;
    
    Task JoinCommunityHubAsync(Guid communityId);
    Task LeaveCommunityHubAsync(Guid communityId);
    
    Task<bool> CreateServerAsync(string name, Stream? avatarStream, string creatorId);

    Task CreateChannelCategoryAsync(string name, Guid communityId);
}