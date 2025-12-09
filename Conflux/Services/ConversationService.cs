using Conflux.Database;
using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Conflux.Services.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace Conflux.Services;

public sealed class ConversationService : IConversationService, IAsyncDisposable {
    private const string MessageSentEventName = "MessageSent";
    private const string MessageDeletedEventName = "MessageDeleted";
    
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly INotificationService _notificationService;
    private readonly NavigationManager _navigationManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ConversationService> _logger;
    private readonly IHubContext<ConversationHub> _hubContext;

    private readonly ConcurrentDictionary<Guid, HubConnection> _conversationHubConnections = [];
    
    public event Action<MessageReceivedEventArgs>? OnMessageReceived;
    public event Action<MessageDeletedEventArgs>? OnMessageDeleted;
    
    public ConversationService(IDbContextFactory<ApplicationDbContext> dbContextFactory, INotificationService notificationService, NavigationManager navigationManager, IHttpContextAccessor httpContextAccessor, IHubContext<ConversationHub> hubContext, IMemoryCache cache, ILogger<ConversationService> logger) {
        _dbContextFactory = dbContextFactory;
        _notificationService = notificationService;
        _navigationManager = navigationManager;
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
        _hubContext = hubContext;
        _logger = logger;
    }

    private HubConnection CreateHubConnectionForConversation(Guid conversationId) {
        return new HubConnectionBuilder()
            .WithUrl(_navigationManager.ToAbsoluteUri($"/hub/conversation?ConversationId={conversationId}"), options => {
                var cookies = _httpContextAccessor.HttpContext!.Request.Cookies.ToDictionary();
                
                options.UseDefaultCredentials = true;
                
                var cookieContainer = cookies.Count != 0 ? new(cookies.Count) : new CookieContainer();

                foreach (var cookie in cookies) {
                    cookieContainer.Add(new Cookie(
                        cookie.Key,
                        WebUtility.UrlEncode(cookie.Value),
                        path: "/",
                        domain: _navigationManager.ToAbsoluteUri("/").Host));
                }

                options.Cookies = cookieContainer;

                foreach (var header in cookies) {
                    options.Headers.Add(header.Key, header.Value);
                }

                options.HttpMessageHandlerFactory = (input) => {
                    var clientHandler = new HttpClientHandler {
                        PreAuthenticate = true,
                        CookieContainer = cookieContainer,
                        UseCookies = true,
                        UseDefaultCredentials = true,
                    };
                    return clientHandler;
                };
            })
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task JoinConversationAsync(Guid conversationId) {
        // TODO: Revise this code due to possible race-condition.
        if (_conversationHubConnections.ContainsKey(conversationId)) return;
        
        var connection = CreateHubConnectionForConversation(conversationId);

        connection.On<MessageReceivedEventArgs>(MessageSentEventName, args => {
            OnMessageReceived?.Invoke(args);
        });

        connection.On<MessageDeletedEventArgs>(MessageDeletedEventName, args => {
            OnMessageDeleted?.Invoke(args);
        });
        
        await connection.StartAsync();
        
        bool add = _conversationHubConnections.TryAdd(conversationId, connection);
        Debug.Assert(add, $"Failed to register hub connection for conversation {conversationId}. Possible race-condition?");
    }

    public async Task LeaveConversationAsync(Guid conversationId) {
        if (_conversationHubConnections.TryRemove(conversationId, out var connection)) {
            await connection.DisposeAsync();
        }
    }

    public async Task<Conversation?> GetOrCreateDirectConversationAsync(string user1, string user2) {
        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync()) {
            string[] userIds = [user1, user2];

            Conversation? conversation = await dbContext.Conversations
                .AsNoTracking()
                .Where(c => c.Type == ConversationType.DirectMessage)
                .Where(c => c.Members.Select(m => m.UserId).Intersect(userIds).Count() == 2)
                .FirstOrDefaultAsync();

            if (conversation != null) {
                return conversation;
            }

            conversation = new() {
                Type = ConversationType.DirectMessage,
            };
            
            dbContext.Add(conversation);
            await dbContext.SaveChangesAsync();

            dbContext.ConversationMembers.AddRange(
                new() {
                    UserId = user1,
                    ConversationId = conversation.Id,
                },
                new() {
                    UserId = user2,
                    ConversationId = conversation.Id,
                }
            );
            
            await dbContext.SaveChangesAsync();

            return conversation;
        }
    }

    public async Task<bool> SendMessageAsync(Guid conversationId, string senderId, string body, Guid? replyMessageId) {
        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync()) {
            ChatMessage message = new() {
                ConversationId = conversationId,
                SenderId = senderId,
                Body = body,
                ReplyMessageId = replyMessageId,
            };
            
            dbContext.ChatMessages.Add(message);

            if (await dbContext.SaveChangesAsync() > 0) {
                await _hubContext.Clients.Group(conversationId.ToString()).SendAsync(MessageSentEventName, new MessageReceivedEventArgs(message));
                
                return true;
            }

            return false;
        }
    }

    public async Task<bool> DeleteMessageAsync(Guid messageId, string senderId) {
        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync()) {
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // Get the conversation ID and check exists at the same time.
            Guid conversationId = await dbContext.ChatMessages
                .Where(m => m.Id == messageId && m.SenderId == senderId && m.DeletedAt == null)
                .Select(m => m.ConversationId)
                .FirstOrDefaultAsync();

            if (conversationId == Guid.Empty) return false;

            DateTime utcNow = DateTime.UtcNow;
            
            int rowsAffected = await dbContext.ChatMessages
                .Where(m => m.Id == messageId && m.SenderId == senderId && m.DeletedAt == null)
                .ExecuteUpdateAsync(builder => {
                    builder.SetProperty(m => m.DeletedAt, utcNow);
                });

            if (rowsAffected > 0) {
                await _hubContext.Clients.Group(conversationId.ToString()).SendAsync(MessageDeletedEventName, new MessageDeletedEventArgs(messageId, conversationId));

                return true;
            }
            
            return false;
        }
    }

    public async Task<IConversationService.RenderingMessages> LoadMessagesBeforeTimestampAsync(Guid conversationId, DateTime beforeTimestamp, int take) {
        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync()) {
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            
            List<IConversationService.RenderingMessageDTO> messages = await dbContext.ChatMessages
                .Where(m => m.ConversationId == conversationId && m.DeletedAt == null)
                .OrderByDescending(m => m.CreatedAt)
                .Where(m => m.CreatedAt < beforeTimestamp)
                .Take(take)
                .Include(m => m.Sender)
                .Select(m => new IConversationService.RenderingMessageDTO(m.Id, m.SenderId, m.Sender.DisplayName, m.Sender.AvatarProfilePath, m.Body, m.CreatedAt, m.ReplyMessageId))
                .Reverse()
                .ToListAsync();

            List<Guid> replyMessageIds = messages.Where(m => m.ReplyMessageId.HasValue).Select(m => m.ReplyMessageId!.Value).ToList();

            List<IConversationService.RenderingReplyMessageDTO> replyMessages = await dbContext.ChatMessages
                .Where(m => replyMessageIds.Contains(m.Id) && m.DeletedAt == null)
                .Include(m => m.Sender)
                .Select(m => new IConversationService.RenderingReplyMessageDTO(m.Id, m.Sender.DisplayName, m.Sender.AvatarProfilePath, m.Body))
                .ToListAsync();
            
            return new(messages, replyMessages);
        }
    }
    
    public async Task<IConversationService.RenderingMessages> LoadMessagesAfterTimestampAsync(Guid conversationId, DateTime beforeTimestamp, int take) {
        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync()) {
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            
            List<IConversationService.RenderingMessageDTO> messages = await dbContext.ChatMessages
                .Where(m => m.ConversationId == conversationId && m.DeletedAt == null)
                .OrderBy(m => m.CreatedAt)
                .Where(m => m.CreatedAt > beforeTimestamp)
                .Take(take)
                .Include(m => m.Sender)
                .Select(m => new IConversationService.RenderingMessageDTO(m.Id, m.SenderId, m.Sender.DisplayName, m.Sender.AvatarProfilePath, m.Body, m.CreatedAt, m.ReplyMessageId))
                .ToListAsync();

            List<Guid> replyMessageIds = messages.Where(m => m.ReplyMessageId.HasValue).Select(m => m.ReplyMessageId!.Value).ToList();

            List<IConversationService.RenderingReplyMessageDTO> replyMessages = await dbContext.ChatMessages
                .Where(m => replyMessageIds.Contains(m.Id) && m.DeletedAt == null)
                .Include(m => m.Sender)
                .Select(m => new IConversationService.RenderingReplyMessageDTO(m.Id, m.Sender.DisplayName, m.Sender.AvatarProfilePath, m.Body))
                .ToListAsync();
            
            return new(messages, replyMessages);
        }
    }

    public async ValueTask DisposeAsync() {
        foreach ((_, HubConnection connection) in _conversationHubConnections) {
            await connection.DisposeAsync();
        }
    }
}