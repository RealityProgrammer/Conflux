namespace Conflux;

public static class ApplicationConstants {
    public const int MaxUserAvatarSize = 128_000;       // 128KB
    public const int MaxServerAvatarSize = 512_000;       // 512KB
    public const int MessageLoadCount = 40;
    public const int MaxAttachmentsPerMessage = 4;
    public const int MaxSizePerAttachment = 24_000_000;   // 24MB, I'm feeling generous.
}