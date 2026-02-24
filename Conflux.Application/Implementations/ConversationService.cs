using Conflux.Application.Abstracts;
using Conflux.Application.Dto;
using Conflux.Domain;
using Conflux.Domain.Entities;
using Conflux.Domain.Enums;
using Conflux.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Conflux.Application.Implementations;

public sealed class ConversationService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IContentService contentService,
    IConversationEventDispatcher conversationEventDispatcher,
    IUserNotificationService userNotificationService,
    ILogger<ConversationService> logger
) : IConversationService {
    public event Action<Conversation>? OnConversationCreated;
    
    public async Task<Conversation> GetOrCreateDirectConversationAsync(Guid friendRequestId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        Conversation? conversation = await dbContext.Conversations
            .Where(c => c.FriendRequestId == friendRequestId)
            .FirstOrDefaultAsync();

        if (conversation != null) {
            return conversation;
        }

        conversation = new() {
            FriendRequestId = friendRequestId,
            CreatedAt = DateTime.UtcNow,
        };

        dbContext.Conversations.Add(conversation);
            
        await dbContext.SaveChangesAsync();
        
        OnConversationCreated?.Invoke(conversation);

        return conversation;
    }

    public async Task<(int TotalCount, List<DirectConversationDisplayDTO> Page)> PaginateDirectConversationDisplayAsync(Guid userId, int startIndex, int count) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var query = dbContext.Conversations
            .Where(x => x.FriendRequestId != null && (x.FriendRequest!.SenderUserId == userId || x.FriendRequest.ReceiverUserId == userId));

        int totalCount = await query.CountAsync();

        if (totalCount == 0) {
            return (0, []);
        }

        List<DirectConversationDisplayDTO> page = await query
            .OrderByDescending(x => x.LatestMessageTime ?? DateTime.MinValue)
            .Join(
                dbContext.Users, 
                conversation => conversation.FriendRequest!.SenderUserId == userId ? conversation.FriendRequest.ReceiverUserId : conversation.FriendRequest.SenderUserId, 
                user => user.Id, 
                (conversation, user) => new DirectConversationDisplayDTO {
                    ConversationId = conversation.Id,
                    OtherUserDisplay = new(user.Id, user.DisplayName, user.UserName, user.AvatarProfilePath)
                }
            )
            .Skip(startIndex)
            .Take(count)
            .ToListAsync();

        return (totalCount, page);
    }

    public async Task<IConversationService.SendStatus> SendMessageAsync(Guid conversationId, Guid senderUserId, string? body, Guid? replyMessageId, IReadOnlyCollection<IConversationService.UploadingAttachment> attachments, CancellationToken cancellationToken = default) {
        // TODO: Rewrite this, this is goddamn ugly.

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        
        // Upload the attachments.
        var attachmentPaths = new List<MessageAttachment>(attachments.Count);

        try {
            foreach (var attachment in attachments) {
                var path = await contentService.UploadMessageAttachmentAsync(attachment.Stream, attachment.Type, cancellationToken);
                
                attachmentPaths.Add(new() {
                    Name = attachment.Name,
                    Type = attachment.Type,
                    PhysicalPath = path,
                });
            }
        } catch (Exception e) {
            logger.LogError(e, "Failed to upload message attachment.");

            for (int i = 0; i < attachmentPaths.Count; i++) {
                await contentService.DeleteMessageAttachmentAsync(attachmentPaths[i].PhysicalPath);
            }
            
            return IConversationService.SendStatus.AttachmentFailure;
        }

        IConversationService.SendStatus returnStatus = IConversationService.SendStatus.Success;

        try {
            // Register the message to database.
            ChatMessage message = new() {
                ConversationId = conversationId,
                SenderUserId = senderUserId,
                Body = body,
                ReplyMessageId = replyMessageId,
                Attachments = attachmentPaths,
                CreatedAt = DateTime.UtcNow,
            };

            dbContext.ChatMessages.Add(message);
            
            // Update the Latest Message Time in Conversation.
            await dbContext.Conversations
                .Where(c => c.Id == conversationId)
                .ExecuteUpdateAsync(builder => {
                    builder.SetProperty(c => c.LatestMessageTime, message.CreatedAt);
                }, cancellationToken);
            
            // Notify the other user if the conversation is direct conversation.
            Guid? otherUserIdOnDirectConversation = await dbContext.Conversations
                .Where(c => c.Id == conversationId && c.FriendRequestId != null)
                .Include(c => c.FriendRequest!)
                .Select(c => c.FriendRequest!.SenderUserId == senderUserId ? c.FriendRequest!.ReceiverUserId : c.FriendRequest!.SenderUserId)
                .Cast<Guid?>()
                .FirstOrDefaultAsync(cancellationToken);

            if (await dbContext.SaveChangesAsync(cancellationToken) > 0) {
                await transaction.CommitAsync(cancellationToken);

                returnStatus = IConversationService.SendStatus.Success;

                await conversationEventDispatcher.Dispatch(new MessageReceivedEventArgs(message.Id, conversationId, senderUserId));

                if (otherUserIdOnDirectConversation != null) {
                    await userNotificationService.Dispatch(new IncomingDirectMessageEventArgs(otherUserIdOnDirectConversation.Value, conversationId, message.Id));
                }
                
                return returnStatus;
            } else {
                returnStatus = IConversationService.SendStatus.MessageFailure;
            }
        } catch (OperationCanceledException) {
            returnStatus = IConversationService.SendStatus.Canceled;
        } catch (Exception e) {
            returnStatus = IConversationService.SendStatus.Failure;
            
            logger.LogError(e, "Failed to send message.");
        } finally {
            if (returnStatus != IConversationService.SendStatus.Success) {
                foreach (var attachmentPath in attachmentPaths) {
                    await contentService.DeleteMessageAttachmentAsync(attachmentPath.PhysicalPath);
                }
            }
        }

        return returnStatus;
    }

    public async Task<bool> DeleteMessageAsync(Guid messageId, Guid deleteUserId) {
        await using (var dbContext = await dbContextFactory.CreateDbContextAsync()) {
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Get the conversation ID and check exists at the same time.
            Guid conversationId = await dbContext.ChatMessages
                .Where(m => m.Id == messageId && m.DeletedAt == null)
                .Select(m => m.ConversationId)
                .FirstOrDefaultAsync();

            if (conversationId == Guid.Empty) return false;

            DateTime utcNow = DateTime.UtcNow;
            
            int rowsAffected = await dbContext.ChatMessages
                .Where(m => m.Id == messageId && m.DeletedAt == null)
                .ExecuteUpdateAsync(builder => {
                    builder.SetProperty(m => m.DeletedAt, utcNow);
                    builder.SetProperty(m => m.DeleterUserId, deleteUserId);
                });

            if (rowsAffected > 0) {
                await conversationEventDispatcher.Dispatch(new MessageDeletedEventArgs(messageId, conversationId));

                return true;
            }
            
            return false;
        }
    }

    public async Task<bool> EditMessageAsync(Guid messageId, string body) {
        await using (var dbContext = await dbContextFactory.CreateDbContextAsync()) {
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Get the conversation ID and check exists at the same time.
            Guid conversationId = await dbContext.ChatMessages
                .Where(m => m.Id == messageId && m.DeletedAt == null)
                .Select(m => m.ConversationId)
                .FirstOrDefaultAsync();

            if (conversationId == Guid.Empty) return false;

            DateTime utcNow = DateTime.UtcNow;
            
            int rowsAffected = await dbContext.ChatMessages
                .Where(m => m.Id == messageId && m.DeletedAt == null)
                .ExecuteUpdateAsync(builder => {
                    builder.SetProperty(m => m.Body, body);
                    builder.SetProperty(m => m.LastModifiedAt, utcNow);
                });

            if (rowsAffected > 0) {
                await conversationEventDispatcher.Dispatch(new MessageEditedEventArgs(messageId, conversationId, body));

                return true;
            }
            
            return false;
        }
    }
    
    public async Task<bool> EditMessageAsync(Guid messageId, Guid senderUserId, string? body) {
        await using (var dbContext = await dbContextFactory.CreateDbContextAsync()) {
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Get the conversation ID and check exists at the same time.
            Guid conversationId = await dbContext.ChatMessages
                .Where(m => m.Id == messageId && m.SenderUserId == senderUserId && m.DeletedAt == null && m.Body != body)
                .Select(m => m.ConversationId)
                .FirstOrDefaultAsync();

            if (conversationId == Guid.Empty) return false;

            DateTime utcNow = DateTime.UtcNow;
            
            int rowsAffected = await dbContext.ChatMessages
                .Where(m => m.Id == messageId && m.SenderUserId == senderUserId && m.DeletedAt == null)
                .ExecuteUpdateAsync(builder => {
                    builder.SetProperty(m => m.Body, body);
                    builder.SetProperty(m => m.LastModifiedAt, utcNow);
                });

            if (rowsAffected > 0) {
                await conversationEventDispatcher.Dispatch(new MessageEditedEventArgs(messageId, conversationId, body));

                return true;
            }
            
            return false;
        }
    }

    public async Task<MessageDisplayDTO?> GetMessageDisplayAsync(Guid messageId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        return await dbContext.ChatMessages
            .Where(m => m.Id == messageId)
            .Include(m => m.Sender)
            .Select(m => new MessageDisplayDTO(m.Sender.DisplayName, m.Sender.AvatarProfilePath, m.Body, m.Attachments))
            .Cast<MessageDisplayDTO?>()
            .FirstOrDefaultAsync();
    }

    public async Task<IConversationService.RenderingMessages> LoadMessagesBeforeTimestampAsync(Guid conversationId, DateTime beforeTimestamp, int take) {
        await using (var dbContext = await dbContextFactory.CreateDbContextAsync()) {
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            
            List<IConversationService.RenderingMessageDTO> messages = await dbContext.ChatMessages
                .Where(m => m.ConversationId == conversationId && m.DeletedAt == null)
                .OrderByDescending(m => m.CreatedAt)
                .Where(m => m.CreatedAt < beforeTimestamp)
                .Take(take)
                .Include(m => m.Sender)
                .Include(m => m.ReplyMessage)
                .Select(m => new IConversationService.RenderingMessageDTO(m.Id, m.SenderUserId, m.Sender.DisplayName, m.Sender.AvatarProfilePath, m.Body, m.CreatedAt, m.LastModifiedAt != null, m.ReplyMessage != null ? m.ReplyMessageId : null, m.Attachments))
                .Reverse()
                .ToListAsync();

            List<Guid> replyMessageIds = messages.Where(m => m.ReplyMessageId.HasValue).Select(m => m.ReplyMessageId!.Value).ToList();

            List<IConversationService.RenderingReplyMessageDTO> replyMessages = await dbContext.ChatMessages
                .Where(m => replyMessageIds.Contains(m.Id) && m.DeletedAt == null)
                .Include(m => m.Sender)
                .Select(m => new IConversationService.RenderingReplyMessageDTO(m.Id, m.Sender.DisplayName, m.Body))
                .ToListAsync();
            
            return new(messages, replyMessages);
        }
    }
    
    public async Task<IConversationService.RenderingMessages> LoadMessagesAfterTimestampAsync(Guid conversationId, DateTime beforeTimestamp, int take) {
        await using (var dbContext = await dbContextFactory.CreateDbContextAsync()) {
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            
            List<IConversationService.RenderingMessageDTO> messages = await dbContext.ChatMessages
                .Where(m => m.ConversationId == conversationId && m.DeletedAt == null)
                .OrderBy(m => m.CreatedAt)
                .Where(m => m.CreatedAt > beforeTimestamp)
                .Take(take)
                .Include(m => m.Sender)
                .Select(m => new IConversationService.RenderingMessageDTO(m.Id, m.SenderUserId, m.Sender.DisplayName, m.Sender.AvatarProfilePath, m.Body, m.CreatedAt, m.LastModifiedAt != null, m.ReplyMessage != null ? m.DeletedAt != null ? m.ReplyMessageId : Guid.Empty : null, m.Attachments))
                .ToListAsync();

            List<Guid> replyMessageIds = messages.Where(m => m.ReplyMessageId.HasValue).Select(m => m.ReplyMessageId!.Value).ToList();

            List<IConversationService.RenderingReplyMessageDTO> replyMessages = await dbContext.ChatMessages
                .Where(m => replyMessageIds.Contains(m.Id) && m.DeletedAt == null)
                .Include(m => m.Sender)
                .Select(m => new IConversationService.RenderingReplyMessageDTO(m.Id, m.Sender.DisplayName, m.Body))
                .ToListAsync();
            
            return new(messages, replyMessages);
        }
    }
}