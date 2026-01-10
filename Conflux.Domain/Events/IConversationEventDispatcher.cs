using Conflux.Domain.Entities;

namespace Conflux.Domain.Events;

public readonly record struct MessageReceivedEventArgs(ChatMessage Message, Guid ConversationId, string SenderId);
public readonly record struct MessageDeletedEventArgs(Guid MessageId, Guid ConversationId);
public readonly record struct MessageEditedEventArgs(Guid MessageId, Guid ConversationId, string? Body);

public interface IConversationEventDispatcher {
    event Action<MessageReceivedEventArgs>? OnMessageReceived;
    event Action<MessageDeletedEventArgs>? OnMessageDeleted;
    event Action<MessageEditedEventArgs>? OnMessageEdited;

    Task Connect(Guid conversationId);
    Task Disconnect(Guid conversationId);

    Task Dispatch(MessageReceivedEventArgs args);
    Task Dispatch(MessageDeletedEventArgs args);
    Task Dispatch(MessageEditedEventArgs args);
}