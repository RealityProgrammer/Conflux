namespace Conflux.Domain.Events;

public readonly record struct MessageDeletedEventArgs(Guid MessageId, Guid ConversationId);
