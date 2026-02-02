using Conflux.Domain.Events;

namespace Conflux.Web.Hubs;

public interface IConversationClient {
    Task MessageReceived(MessageReceivedEventArgs args);
    Task MessageDeleted(MessageDeletedEventArgs args);
    Task MessageEdited(MessageEditedEventArgs args);
}