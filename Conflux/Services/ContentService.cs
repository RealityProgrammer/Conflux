using Conflux.Services.Abstracts;

namespace Conflux.Services;

public class ContentService(IWebHostEnvironment environment) : IContentService {
    public async Task<string> UploadAvatarAsync(Stream stream, string userId, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();

        string avatarDirectory = Path.Combine(environment.ContentRootPath, "Uploads", "avatars");

        if (!Directory.Exists(avatarDirectory)) {
            Directory.CreateDirectory(avatarDirectory);
        }
        
        string path = Path.Combine("avatars", userId);
        string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);
        
        await using var destinationStream = File.OpenWrite(physicalPath);
        destinationStream.SetLength(0);
        
        await destinationStream.FlushAsync(cancellationToken);
        await stream.CopyToAsync(destinationStream, cancellationToken);

        return path;
    }

    public Task DeleteAvatarAsync(string userId) {
        try {
            string path = Path.Combine("avatars", userId);
            string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);

            File.Delete(physicalPath);
        } catch (Exception e) when (e is DirectoryNotFoundException or IOException or PathTooLongException) {
        }
        
        return Task.CompletedTask;
    }

    public DateTime GetAvatarUploadTime(string userId) {
        try {
            string path = Path.Combine("avatars", userId);
            string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);

            return File.GetLastWriteTimeUtc(physicalPath);
        } catch (Exception e) when (e is DirectoryNotFoundException or IOException or PathTooLongException) {
            return default;
        }
    }

    public async Task<string> UploadMessageAttachmentAsync(Stream attachmentStream, CancellationToken cancellationToken) {
        string avatarDirectory = Path.Combine(environment.ContentRootPath, "Uploads", "msg_attachments");

        if (!Directory.Exists(avatarDirectory)) {
            Directory.CreateDirectory(avatarDirectory);
        }
        
        string path = Path.Combine("msg_attachments", Guid.CreateVersion7().ToString());
        string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);

        await using var destinationStream = File.OpenWrite(physicalPath);
        destinationStream.SetLength(0);

        await destinationStream.FlushAsync(cancellationToken);
        await attachmentStream.CopyToAsync(destinationStream, cancellationToken);

        return path;
    }

    public async Task<ICollection<string>> UploadMessageAttachmentsAsync(IReadOnlyCollection<Stream> attachmentStreams, CancellationToken cancellationToken = default) {
        List<string> outputPaths = new List<string>(attachmentStreams.Count);

        try {
            foreach (var stream in attachmentStreams) {
                outputPaths.Add(await UploadMessageAttachmentAsync(stream, cancellationToken));
            }

            return outputPaths;
        } catch {
            foreach (string path in outputPaths) {
                string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);
    
                File.Delete(physicalPath);
            }

            throw;
        }
    }

    public Task<bool> DeleteMessageAttachmentAsync(string attachmentRelativePath) {
        try {
            string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", attachmentRelativePath);

            if (!attachmentRelativePath.StartsWith(Path.Combine(environment.ContentRootPath, "Uploads", "msg_attachments"))) {
                return Task.FromResult(false);
            }

            File.Delete(physicalPath);

            return Task.FromResult(true);
        } catch {
            return Task.FromResult(false);
        }
    }

    public string GetAssetPath(string resourcePath) {
        return "uploads/" + resourcePath;
    }
}