namespace Conflux.Services.Abstracts;

public interface IContentService {
    Task<string> UploadAvatarAsync(Stream stream, string userId);
    Task DeleteAvatarAsync(string userId);
}