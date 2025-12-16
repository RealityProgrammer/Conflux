namespace Conflux.Services.Abstracts;

public interface ICommunityServerService {
    Task<bool> CreateServerAsync(string name, Stream? avatarStream, string creatorId);
}