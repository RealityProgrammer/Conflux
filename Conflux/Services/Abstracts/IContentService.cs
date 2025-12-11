namespace Conflux.Services.Abstracts;

public interface IContentService {
    Task<string> UploadAvatarAsync(Stream stream, string userId, CancellationToken cancellationToken = default);
    Task DeleteAvatarAsync(string userId);
    DateTime GetAvatarUploadTime(string userId);

    Task<ICollection<string>> UploadMessageAttachmentsAsync(ICollection<Stream> attachmentStreams, CancellationToken cancellationToken = default);
    Task<bool> DeleteMessageAttachmentAsync(string attachmentRelativePath);
}