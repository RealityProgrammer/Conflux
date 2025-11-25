namespace Conflux.Components.Pages.Lobby.User.Social;

public class UserSearchResult {
    public required string UserName { get; init; }
    public required string DisplayName { get; init; }
    public required string? AvatarImagePath { get; init; }
    public required double AvatarScaleX { get; init; }
    public required double AvatarScaleY { get; init; }
    public required bool CanSendFriendRequest { get; init; }
}