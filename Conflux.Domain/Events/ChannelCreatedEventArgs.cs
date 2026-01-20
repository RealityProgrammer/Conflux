using Conflux.Domain.Entities;
using Conflux.Domain.Enums;

namespace Conflux.Domain.Events;

public readonly record struct ChannelCreatedEventArgs(Guid CommunityId, Guid CategoryId, Guid ChannelId, string ChannelName, CommunityChannelType ChannelType);