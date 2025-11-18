using Conflux.Components.Services.Abstracts;

namespace Conflux.Components.Services;

public class ContentService(IWebHostEnvironment environment) : IContentService {
    public string GetAbsoluteContentPath(string relativePath) {
        return Path.Combine(environment.WebRootPath, relativePath);
    }

    public string GetAbsoluteContentPath(string purpose, string type, string path) {
        return Path.Combine(environment.WebRootPath, purpose, type, path);
    }

    public string GetRelativeContentPath(string purpose, string resourceType, string path) {
        return Path.Join(purpose, resourceType, path);
    }
}