using Conflux.Services.Abstracts;

namespace Conflux.Services;

public class ContentService(IWebHostEnvironment environment) : IContentService {
    public async Task<string> UploadAvatarAsync(Stream stream, string userId) {
        string path = Path.Combine("images", "avatar", userId);
        string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);
        await using var destinationStream = File.OpenWrite(physicalPath);
        
        await stream.CopyToAsync(destinationStream);

        return Path.Join("uploads", path);
    }

    public Task DeleteAvatarAsync(string userId) {
        string path = Path.Combine("images", "avatar", userId);
        string physicalPath = Path.Combine(environment.ContentRootPath, "Uploads", path);
        
        File.Delete(physicalPath);

        return Task.CompletedTask;
    }
}