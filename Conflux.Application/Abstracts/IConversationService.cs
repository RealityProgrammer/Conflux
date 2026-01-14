using Conflux.Domain.Entities;

namespace Conflux.Application.Abstracts;

public interface IConversationService {
    Task<Conversation> GetOrCreateDirectConversationAsync(Guid friendRequestId);
    
    Task<SendStatus> SendMessageAsync(Guid conversationId, string senderId, string? body, Guid? replyMessageId, IReadOnlyCollection<UploadingAttachment> attachments, CancellationToken cancellationToken = default);
    Task<bool> DeleteMessageAsync(Guid messageId, string senderId);
    Task<bool> EditMessageAsync(Guid messageId, string body);
    Task<bool> EditMessageAsync(Guid messageId, string senderId, string? body);

    Task<RenderingMessages> LoadMessagesBeforeTimestampAsync(Guid conversationId, DateTime beforeTimestamp, int take);
    Task<RenderingMessages> LoadMessagesAfterTimestampAsync(Guid conversationId, DateTime afterTimestamp, int take);

    public record RenderingMessageDTO(Guid MessageId, string SenderId, string SenderDisplayName, string? SenderAvatar, string? Body, DateTime CreatedAt, bool IsEdited, Guid? ReplyMessageId, List<MessageAttachment> Attachments);
    public record RenderingReplyMessageDTO(Guid MessageId, string SenderDisplayName, string? Body);
    public readonly record struct RenderingMessages(IList<RenderingMessageDTO> VisibleMessages, IList<RenderingReplyMessageDTO> RepliedMessages);
    public readonly record struct UploadingAttachment(string Name, MessageAttachmentType Type, Stream Stream);
    
    public enum SendStatus {
        Success,
        AttachmentFailure,
        MessageFailure,
        Failure,
        Canceled,
    }
}