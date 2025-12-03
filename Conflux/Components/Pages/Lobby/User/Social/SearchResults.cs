namespace Conflux.Components.Pages.Lobby.User.Social;

public enum FriendStatus {
    Stranger,
    OutcomingPending,
    IncomingPending,
    Friend,
}

public class UserSearchResult {
    public required string Id { get; set; }
    public required string UserName { get; set; }
    public required string DisplayName { get; set; }
    public required string? AvatarImagePath { get; set; }
    public required double AvatarScaleX { get; set; }
    public required double AvatarScaleY { get; set; }
    public FriendStatus FriendStatus { get; set; }
}