using Conflux.Database.Entities;

namespace Conflux.Services.Abstracts;

public readonly record struct MessageReceivedEventArgs(ChatMessage Message);
public readonly record struct MessageDeletedEventArgs(Guid MessageId, Guid ConversationId);

public interface IConversationService {
    event Action<MessageReceivedEventArgs>? OnMessageReceived;
    event Action<MessageDeletedEventArgs>? OnMessageDeleted;
    
    Task JoinConversationAsync(Guid conversationId);
    Task LeaveConversationAsync(Guid conversationId);
    
    Task<Conversation?> GetOrCreateDirectConversationAsync(string user1, string user2);
    
    Task<bool> SendMessageAsync(Guid conversationId, string senderId, string body, Guid? replyMessageId);
    Task<bool> DeleteMessageAsync(Guid messageId, string senderId);

    Task<RenderingMessages> LoadMessagesBeforeTimestampAsync(Guid conversationId, DateTime beforeTimestamp, int take);
    Task<RenderingMessages> LoadMessagesAfterTimestampAsync(Guid conversationId, DateTime afterTimestamp, int take);
    
    public record RenderingMessageDTO(Guid MessageId, string SenderId, string SenderDisplayName, string? SenderAvatar, string Body, DateTime CreatedAt, Guid? ReplyMessageId);

    public record RenderingReplyMessageDTO(Guid MessageId, string SenderDisplayName, string? SenderAvatar, string Body);
    
    public readonly record struct RenderingMessages(IList<RenderingMessageDTO> VisibleMessages, IList<RenderingReplyMessageDTO> RepliedMessages);
}