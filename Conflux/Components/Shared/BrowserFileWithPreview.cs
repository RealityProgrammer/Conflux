using Microsoft.AspNetCore.Components.Forms;

namespace Conflux.Components.Shared;

public readonly record struct BrowserFileWithPreview(IBrowserFile File, string PreviewUrl);