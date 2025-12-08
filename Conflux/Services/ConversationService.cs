using Conflux.Database;
using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Conflux.Services.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace Conflux.Services;

public sealed class ConversationService : IConversationService, IAsyncDisposable {
    private const string MessageSentEventName = "MessageSent";
    
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly INotificationService _notificationService;
    private readonly NavigationManager _navigationManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ConversationService> _logger;
    private readonly IHubContext<ConversationHub> _hubContext;

    private readonly ConcurrentDictionary<Guid, HubConnection> _conversationHubConnections = [];
    
    public event Action<MessageReceivedEventArgs>? OnMessageReceived;
    
    public ConversationService(IWebHostEnvironment environment, IDbContextFactory<ApplicationDbContext> dbContextFactory, INotificationService notificationService, NavigationManager navigationManager, IHttpContextAccessor httpContextAccessor, IHubContext<ConversationHub> hubContext, ILogger<ConversationService> logger) {
        _dbContextFactory = dbContextFactory;
        _notificationService = notificationService;
        _navigationManager = navigationManager;
        _httpContextAccessor = httpContextAccessor;
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
                ReplyMessageId = null,
            };
            
            dbContext.ChatMessages.Add(message);

            if (await dbContext.SaveChangesAsync() > 0) {
                await _hubContext.Clients.Group(conversationId.ToString()).SendAsync(MessageSentEventName, new MessageReceivedEventArgs(message));
                
                return true;
            }

            return false;
        }
    }

    public async ValueTask DisposeAsync() {
        foreach ((_, HubConnection connection) in _conversationHubConnections) {
            await connection.DisposeAsync();
        }
    }
}