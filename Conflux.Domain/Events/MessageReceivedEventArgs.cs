namespace Conflux.Domain.Events;

public readonly record struct MessageReceivedEventArgs(Guid MessageId, Guid ConversationId, Guid SenderId);
