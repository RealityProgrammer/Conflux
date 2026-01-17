namespace Conflux.Domain.Events;

public readonly record struct MessageEditedEventArgs(Guid MessageId, Guid ConversationId, string? Body);
