using Conflux.Database.Entities;

namespace Conflux.Services.Abstracts;

public interface IContentService {
    Task<string> UploadUserAvatarAsync(Stream stream, string userId, CancellationToken cancellationToken = default);
    Task DeleteUserAvatarAsync(string userId);
    DateTime GetUserAvatarUploadTime(string userId);

    Task<string> UploadMessageAttachmentAsync(Stream attachmentStream, MessageAttachmentType type, CancellationToken cancellationToken);
    Task<bool> DeleteMessageAttachmentAsync(string attachmentRelativePath);
    
    Task<string> UploadCommunityAvatarAsync(Stream stream, Guid serverId, CancellationToken cancellationToken = default);
    Task DeleteCommunityAvatarAsync(Guid serverId);
    
    Task<string> UploadCommunityBannerAsync(Stream stream, Guid serverId, CancellationToken cancellationToken = default);
    Task DeleteCommunityBannerAsync(Guid serverId);
    
    string GetAssetPath(string resourcePath);
}