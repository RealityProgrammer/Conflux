using Conflux.Database.Entities;

namespace Conflux.Services.Abstracts;

public interface IConversationService {
    Task JoinConversationAsync(Guid conversationId);
    Task LeaveConversationAsync(Guid conversationId);
    
    Task<Conversation?> GetOrCreateDirectConversationAsync(string user1, string user2);
    
    Task<bool> SendMessageAsync(Guid conversationId, string senderId, string body, Guid? replyMessageId);
}