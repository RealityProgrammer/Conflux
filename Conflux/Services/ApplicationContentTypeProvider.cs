using Microsoft.AspNetCore.StaticFiles;
using System.Diagnostics.CodeAnalysis;

namespace Conflux.Services;

internal sealed class ApplicationContentTypeProvider : IContentTypeProvider {
    public bool TryGetContentType(string subpath, [MaybeNullWhen(false)] out string contentType) {
        if (subpath.StartsWith("avatars") || subpath.StartsWith("/avatars")) {
            contentType = "image/*";
            return true;
        }

        if (subpath.StartsWith("msg_attachments") || subpath.StartsWith("/msg_attachments")) {
            contentType = "image/*";
            return true;
        }

        contentType = null;
        return false;
    }
}