using Conflux.Domain.Events;

namespace Conflux.Application.Abstracts;

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