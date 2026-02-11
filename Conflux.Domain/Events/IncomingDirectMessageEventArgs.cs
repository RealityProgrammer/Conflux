namespace Conflux.Domain.Events;

public readonly record struct IncomingDirectMessageEventArgs(Guid TargetUserId, Guid ConversationId, Guid MessageId);