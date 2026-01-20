using Microsoft.AspNetCore.Components.Forms;

namespace Conflux.Web.Components.Shared;

public enum ImageUploadOperation {
    Nothing,
    Upload,
    Delete,
}

public readonly record struct ImageUploading(ImageUploadOperation Operation, IBrowserFile? File, string? PreviewUrl);