namespace Conflux.Domain.Events;

public readonly record struct ChannelCategoryCreatedEventArgs(Guid CommunityId, Guid CategoryId, string CategoryName);