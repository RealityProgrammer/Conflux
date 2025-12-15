using Microsoft.AspNetCore.Components;

namespace Conflux.Services.Abstracts;

public enum SnackbarType {
    Info,
    Success,
    Warning,
    Error,
}

public struct SnackbarOptions {
    public string? Text { get; set; }
    public MarkupString? Markup { get; set; }
    public RenderFragment? RenderFragment { get; set; }
    
    public required TimeSpan Duration { get; set; }
    public required SnackbarType Type { get; set; }
    
    public Action? OnClick { get; set; }
}