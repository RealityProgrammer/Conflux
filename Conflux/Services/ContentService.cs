using Conflux.Services.Abstracts;

namespace Conflux.Services;

public class ContentService(IWebHostEnvironment environment) : IContentService {
    public async Task<string> UploadAvatarAsync(Stream stream, string userId, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        
        string path = Path.Combine("avatar", userId);
        string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);
        
        await using var destinationStream = File.OpenWrite(physicalPath);
        destinationStream.SetLength(0);
        
        await destinationStream.FlushAsync(cancellationToken);
        await stream.CopyToAsync(destinationStream, cancellationToken);

        return path;
    }

    public Task DeleteAvatarAsync(string userId) {
        string path = Path.Combine("avatar", userId);
        string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);
        
        File.Delete(physicalPath);

        return Task.CompletedTask;
    }

    public DateTime GetAvatarUploadTime(string userId) {
        string path = Path.Combine("avatar", userId);
        string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);
        
        return File.GetLastWriteTimeUtc(physicalPath);
    }

    public async Task<ICollection<string>> UploadMessageAttachmentsAsync(ICollection<Stream> attachmentStreams, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        
        List<string> outputPaths = new List<string>(attachmentStreams.Count);

        try {
            foreach (Stream stream in attachmentStreams) {
                cancellationToken.ThrowIfCancellationRequested();
                
                string path = Path.Combine("msg_attachments", Guid.CreateVersion7().ToString());
                string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);

                await using var destinationStream = File.OpenWrite(physicalPath);
                destinationStream.SetLength(0);

                await destinationStream.FlushAsync(cancellationToken);
                await stream.CopyToAsync(destinationStream, cancellationToken);

                outputPaths.Add(path);
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
        string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", attachmentRelativePath);

        if (!attachmentRelativePath.StartsWith(Path.Combine(environment.ContentRootPath, "Uploads", "msg_attachments"))) {
            return Task.FromResult(false);
        }
        
        File.Delete(physicalPath);

        return Task.FromResult(true);
    }
}