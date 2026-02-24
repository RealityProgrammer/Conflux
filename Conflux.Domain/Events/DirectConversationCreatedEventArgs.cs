namespace Conflux.Domain.Events;

public readonly record struct DirectConversationCreatedEventArgs(Guid UserId, Guid ConversationId);