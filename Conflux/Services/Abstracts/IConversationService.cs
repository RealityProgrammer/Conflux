using Conflux.Database.Entities;

namespace Conflux.Services.Abstracts;

public readonly record struct MessageReceivedEventArgs(ChatMessage Message);

public interface IConversationService {
    event Action<MessageReceivedEventArgs>? OnMessageReceived;
    
    Task JoinConversationAsync(Guid conversationId);
    Task LeaveConversationAsync(Guid conversationId);
    
    Task<Conversation?> GetOrCreateDirectConversationAsync(string user1, string user2);
    
    Task<bool> SendMessageAsync(Guid conversationId, string senderId, string body, Guid? replyMessageId);
}