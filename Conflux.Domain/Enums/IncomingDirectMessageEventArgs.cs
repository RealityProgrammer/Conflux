namespace Conflux.Domain.Enums;

public readonly record struct IncomingDirectMessageEventArgs(Guid TargetUserId, Guid ConversationId, Guid MessageId);