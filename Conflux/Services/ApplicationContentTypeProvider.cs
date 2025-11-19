using Microsoft.AspNetCore.StaticFiles;
using System.Diagnostics.CodeAnalysis;

namespace Conflux.Services;

internal sealed class ApplicationContentTypeProvider : IContentTypeProvider {
    public bool TryGetContentType(string subpath, [MaybeNullWhen(false)] out string contentType) {
        if (subpath.StartsWith("images/avatar") || subpath.StartsWith("/images/avatar")) {
            contentType = "image/*";
            return true;
        }

        contentType = null;
        return false;
    }
}