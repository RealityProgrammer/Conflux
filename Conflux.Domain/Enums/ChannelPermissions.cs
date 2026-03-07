namespace Conflux.Domain.Enums;

[Flags]
public enum ChannelPermissions : byte
{
    None = 0,

    CreateChannelCategory = 1 << 0,
    DeleteChannelCategory = 1 << 1,

    CreateChannel = 1 << 2,
    DeleteChannel = 1 << 3,

    All = CreateChannelCategory | DeleteChannelCategory | CreateChannel | DeleteChannel,
}