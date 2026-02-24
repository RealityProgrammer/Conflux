using Conflux.Application.Dto;
using Conflux.Domain.Entities;
using Conflux.Domain.Enums;

namespace Conflux.Application.Abstracts;

public interface IConversationService {
    event Action<Conversation> OnConversationCreated;
    
    Task<Conversation> GetOrCreateDirectConversationAsync(Guid friendRequestId, Guid creatorUserId);
    
    Task<SendStatus> SendMessageAsync(Guid conversationId, Guid senderUserId, string? body, Guid? replyMessageId, IReadOnlyCollection<UploadingAttachment> attachments, CancellationToken cancellationToken = default);
    Task<bool> DeleteMessageAsync(Guid messageId, Guid deleteUserId);
    Task<bool> EditMessageAsync(Guid messageId, string body);
    Task<bool> EditMessageAsync(Guid messageId, Guid senderUserId, string? body);
    
    Task<MessageDisplayDTO?> GetMessageDisplayAsync(Guid messageId);
    
    Task<RenderingMessages> LoadMessagesBeforeTimestampAsync(Guid conversationId, DateTime beforeTimestamp, int take);
    Task<RenderingMessages> LoadMessagesAfterTimestampAsync(Guid conversationId, DateTime afterTimestamp, int take);

    Task<(int TotalCount, List<DirectConversationDisplayDTO> Page)> PaginateDirectConversationDisplayAsync(Guid userId, int startIndex, int count);
    
    public record RenderingMessageDTO(Guid MessageId, Guid SenderUserId, string SenderDisplayName, string? SenderAvatar, string? Body, DateTime CreatedAt, bool IsEdited, Guid? ReplyMessageId, List<MessageAttachment> Attachments);
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