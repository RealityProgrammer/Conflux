namespace Conflux.Web;

public static class ApplicationConstants {
    public const int MaxUserAvatarSize = 128_000;       // 128KB
    public const int MaxCommunityAvatarSize = 512_000;  // 512KB
    public const int MaxCommunityBannerSize = 512_000;  // 512KB
    public const int MessageLoadCount = 40;
    public const int MaxAttachmentsPerMessage = 4;
    public const int MaxSizePerAttachment = 20_000_000; // 20MB, I'm feeling generous.
}