using Microsoft.AspNetCore.StaticFiles;
using System.Diagnostics.CodeAnalysis;

namespace Conflux.Web.Services;

internal sealed class ApplicationContentTypeProvider : IContentTypeProvider {
    public bool TryGetContentType(string subpath, [MaybeNullWhen(false)] out string contentType) {
        if (subpath.StartsWith("communities/avatars") || 
            subpath.StartsWith("/communities/avatars") ||
            subpath.StartsWith("communities/banners") ||
            subpath.StartsWith("/communities/banners") ||
            subpath.StartsWith("users/avatars") ||
            subpath.StartsWith("/users/avatars")
        ) {
            contentType = "image/*";
            return true;
        }

        if (subpath.StartsWith("attachments/images") || subpath.StartsWith("/attachments/images")) {
            contentType = "image/*";
            return true;
        }
        
        if (subpath.StartsWith("attachments/audios") || subpath.StartsWith("/attachments/audios")) {
            contentType = "audio/*";
            return true;
        }
        
        if (subpath.StartsWith("attachments/videos") || subpath.StartsWith("/attachments/videos")) {
            contentType = "video/*";
            return true;
        }

        contentType = null;
        return false;
    }
}