namespace Conflux.Services.Abstracts;

public interface IContentService {
    string GetAbsoluteContentPath(string relativePath);
    string GetAbsoluteContentPath(string purpose, string resourceType, string path);
    string GetRelativeContentPath(string purpose, string resourceType, string path);
}