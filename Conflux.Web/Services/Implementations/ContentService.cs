using Conflux.Application.Abstracts;
using Conflux.Domain.Entities;

namespace Conflux.Services.Implementations;

public class ContentService(IWebHostEnvironment environment, ILogger<ContentService> logger) : IContentService {
    public async Task<string> UploadUserAvatarAsync(Stream stream, string userId, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();

        string avatarDirectory = Path.Combine(environment.ContentRootPath, "Uploads", "users", "avatars");

        if (!Directory.Exists(avatarDirectory)) {
            Directory.CreateDirectory(avatarDirectory);
        }
        
        string path = Path.Combine("users", "avatars", userId);
        string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);
        
        await using var destinationStream = File.OpenWrite(physicalPath);
        destinationStream.SetLength(0);
        
        await destinationStream.FlushAsync(cancellationToken);
        await stream.CopyToAsync(destinationStream, cancellationToken);

        return path;
    }

    public Task DeleteUserAvatarAsync(string userId) {
        try {
            string path = Path.Combine("users", "avatars", userId);
            string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);

            File.Delete(physicalPath);
        } catch (Exception e) when (e is DirectoryNotFoundException or IOException or PathTooLongException) {
        }
        
        return Task.CompletedTask;
    }

    public DateTime GetUserAvatarUploadTime(string userId) {
        try {
            string path = Path.Combine("users", "avatars", userId);
            string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);

            return File.GetLastWriteTimeUtc(physicalPath);
        } catch (Exception e) when (e is DirectoryNotFoundException or IOException or PathTooLongException) {
            return default;
        }
    }

    public async Task<string> UploadMessageAttachmentAsync(Stream attachmentStream, MessageAttachmentType type, CancellationToken cancellationToken) {
        try {
            logger.LogInformation("Uploading message attachment type {t}.", type);
            
            string typePath = type switch {
                MessageAttachmentType.Image => "images",
                MessageAttachmentType.Audio => "audios",
                MessageAttachmentType.Video => "videos",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            string avatarDirectory = Path.Combine(environment.ContentRootPath, "Uploads", "attachments", typePath);

            if (!Directory.Exists(avatarDirectory)) {
                Directory.CreateDirectory(avatarDirectory);
            }

            string path = Path.Combine("attachments", typePath, Guid.CreateVersion7().ToString());
            string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);

            await using var destinationStream = File.OpenWrite(physicalPath);
            destinationStream.SetLength(0);
            
            logger.LogInformation("Attachment stream length: {l}.", attachmentStream.Length);

            await destinationStream.FlushAsync(cancellationToken);
            await attachmentStream.CopyToAsync(destinationStream, cancellationToken);

            return path;
        } catch (Exception e) {
            logger.LogInformation(e, "Failed to upload message attachment type {t}.", type);
            logger.LogError(e, "Failed to upload message attachment type {t}.", type);
            return string.Empty;
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
    
    public async Task<string> UploadCommunityAvatarAsync(Stream stream, Guid communityId, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();

        string avatarDirectory = Path.Combine(environment.ContentRootPath, "Uploads", "communities", "avatars");

        if (!Directory.Exists(avatarDirectory)) {
            Directory.CreateDirectory(avatarDirectory);
        }
        
        string path = Path.Combine("communities", "avatars", communityId.ToString());
        string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);
        
        await using var destinationStream = File.OpenWrite(physicalPath);
        destinationStream.SetLength(0);
        
        await destinationStream.FlushAsync(cancellationToken);
        await stream.CopyToAsync(destinationStream, cancellationToken);

        return path;
    }

    public Task DeleteCommunityAvatarAsync(Guid communityId) {
        try {
            string path = Path.Combine("communities", "avatars", communityId.ToString());
            string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);

            File.Delete(physicalPath);
        } catch (Exception e) when (e is DirectoryNotFoundException or IOException or PathTooLongException) {
        }
        
        return Task.CompletedTask;
    }
    
    public async Task<string> UploadCommunityBannerAsync(Stream stream, Guid communityId, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();

        string avatarDirectory = Path.Combine(environment.ContentRootPath, "Uploads", "communities", "banners");

        if (!Directory.Exists(avatarDirectory)) {
            Directory.CreateDirectory(avatarDirectory);
        }
        
        string path = Path.Combine("communities", "banners", communityId.ToString());
        string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);
        
        await using var destinationStream = File.OpenWrite(physicalPath);
        destinationStream.SetLength(0);
        
        await destinationStream.FlushAsync(cancellationToken);
        await stream.CopyToAsync(destinationStream, cancellationToken);

        return path;
    }

    public Task DeleteCommunityBannerAsync(Guid communityId) {
        try {
            string path = Path.Combine("communities", "banners", communityId.ToString());
            string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);

            File.Delete(physicalPath);
        } catch (Exception e) when (e is DirectoryNotFoundException or IOException or PathTooLongException) {
        }
        
        return Task.CompletedTask;
    }

    public string GetAssetPath(string resourcePath) {
        return "uploads/" + resourcePath;
    }
}