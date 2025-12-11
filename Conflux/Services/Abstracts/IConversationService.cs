using Conflux.Database.Entities;

namespace Conflux.Services.Abstracts;

// TODO: Revision required: Should we send the whole structure, or only the message ID.
public readonly record struct MessageReceivedEventArgs(Guid MessageId, Guid ConversationId, string SenderId);
public readonly record struct MessageDeletedEventArgs(Guid MessageId, Guid ConversationId);
public readonly record struct MessageEditedEventArgs(Guid MessageId, Guid ConversationId, string Body);

public interface IConversationService {
    event Action<MessageReceivedEventArgs>? OnMessageReceived;
    event Action<MessageDeletedEventArgs>? OnMessageDeleted;
    event Action<MessageEditedEventArgs>? OnMessageEdited;
    
    Task JoinConversationAsync(Guid conversationId);
    Task LeaveConversationAsync(Guid conversationId);
    
    Task<Conversation?> GetOrCreateDirectConversationAsync(string user1, string user2);
    
    Task<SendStatus> SendMessageAsync(Guid conversationId, string senderId, string body, Guid? replyMessageId, IReadOnlyCollection<UploadingAttachment> attachments, CancellationToken cancellationToken = default);
    Task<bool> DeleteMessageAsync(Guid messageId, string senderId);
    Task<bool> EditMessageAsync(Guid messageId, string body);
    Task<bool> EditMessageAsync(Guid messageId, string senderId, string body);

    Task<RenderingMessages> LoadMessagesBeforeTimestampAsync(Guid conversationId, DateTime beforeTimestamp, int take);
    Task<RenderingMessages> LoadMessagesAfterTimestampAsync(Guid conversationId, DateTime afterTimestamp, int take);
    
    public record RenderingMessageDTO(Guid MessageId, string SenderId, string SenderDisplayName, string? SenderAvatar, string? Body, DateTime CreatedAt, bool IsEdited, Guid? ReplyMessageId, string[] Attachments);

    public record RenderingReplyMessageDTO(Guid MessageId, string SenderDisplayName, string? Body);
    
    public readonly record struct RenderingMessages(IList<RenderingMessageDTO> VisibleMessages, IList<RenderingReplyMessageDTO> RepliedMessages);

    public readonly record struct UploadingAttachment(string Name, Stream Stream);
    
    public enum SendStatus {
        Success,
        AttachmentFailure,
        MessageFailure,
        Failure,
        Canceled,
    }
}