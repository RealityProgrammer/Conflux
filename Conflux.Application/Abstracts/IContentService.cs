using Conflux.Domain.Entities;

namespace Conflux.Application.Abstracts;

public interface IContentService {
    Task<string> UploadUserAvatarAsync(Stream stream, string userId, CancellationToken cancellationToken = default);
    Task DeleteUserAvatarAsync(string userId);
    DateTime GetUserAvatarUploadTime(string userId);

    Task<string> UploadMessageAttachmentAsync(Stream attachmentStream, MessageAttachmentType type, CancellationToken cancellationToken);
    Task<bool> DeleteMessageAttachmentAsync(string attachmentRelativePath);
    
    Task<string> UploadCommunityAvatarAsync(Stream stream, Guid communityId, CancellationToken cancellationToken = default);
    Task DeleteCommunityAvatarAsync(Guid communityId);
    
    Task<string> UploadCommunityBannerAsync(Stream stream, Guid communityId, CancellationToken cancellationToken = default);
    Task DeleteCommunityBannerAsync(Guid communityId);
    
    string GetAssetPath(string resourcePath);
}