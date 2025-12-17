namespace Conflux.Services.Abstracts;

public interface ICommunityService {
    Task<bool> CreateServerAsync(string name, Stream? avatarStream, string creatorId);

    Task CreateChannelCategoryAsync(string name, Guid serverId);
}