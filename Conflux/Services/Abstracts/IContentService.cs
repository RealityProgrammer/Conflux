using Conflux.Database.Entities;

namespace Conflux.Services.Abstracts;

public interface IContentService {
    Task<string> UploadAvatarAsync(Stream stream, string userId, CancellationToken cancellationToken = default);
    Task DeleteAvatarAsync(string userId);
    DateTime GetAvatarUploadTime(string userId);

    Task<string> UploadMessageAttachmentAsync(Stream attachmentStream, MessageAttachmentType type, CancellationToken cancellationToken);
    Task<bool> DeleteMessageAttachmentAsync(string attachmentRelativePath);

    string GetAssetPath(string resourcePath);
}