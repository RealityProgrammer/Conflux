using Conflux.Domain.Events;
using Conflux.Services.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace Conflux.Services;

public sealed class ConversationEventDispatcher(
    IHubContext<ConversationHub> hubContext, 
    IHttpContextAccessor httpContextAccessor, 
    NavigationManager navigationManager
) : IConversationEventDispatcher, IAsyncDisposable {
    private const string MessageReceivedEventName = "MessageReceived";
    private const string MessageDeletedEventName = "MessageDeleted";
    private const string MessageEditedEventName = "MessageEdited";
    
    public event Action<MessageReceivedEventArgs>? OnMessageReceived;
    public event Action<MessageDeletedEventArgs>? OnMessageDeleted;
    public event Action<MessageEditedEventArgs>? OnMessageEdited;
    
    private readonly Dictionary<Guid, HubConnection> _hubConnections = [];
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    
    private HubConnection CreateHubConnection(Guid conversationId) {
        return new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri($"/hub/conversation?ConversationId={conversationId}"), options => {
                var cookies = httpContextAccessor.HttpContext!.Request.Cookies.ToDictionary();
                
                options.UseDefaultCredentials = true;
                
                var cookieContainer = cookies.Count != 0 ? new(cookies.Count) : new CookieContainer();

                foreach (var cookie in cookies) {
                    cookieContainer.Add(new Cookie(
                        cookie.Key,
                        WebUtility.UrlEncode(cookie.Value),
                        path: "/",
                        domain: navigationManager.ToAbsoluteUri("/").Host));
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

    public async Task Connect(Guid conversationId) {
        await _connectionLock.WaitAsync();

        try {
            if (_hubConnections.ContainsKey(conversationId)) return;

            var connection = CreateHubConnection(conversationId);

            connection.On<MessageReceivedEventArgs>(MessageReceivedEventName, args => { OnMessageReceived?.Invoke(args); });
            connection.On<MessageDeletedEventArgs>(MessageDeletedEventName, args => { OnMessageDeleted?.Invoke(args); });
            connection.On<MessageEditedEventArgs>(MessageEditedEventName, args => { OnMessageEdited?.Invoke(args); });

            await connection.StartAsync();

            _hubConnections.Add(conversationId, connection);
        } finally {
            _connectionLock.Release();
        }
    }

    public async Task Disconnect(Guid conversationId) {
        await _connectionLock.WaitAsync();

        try {
            if (_hubConnections.Remove(conversationId, out var connection)) {
                await connection.DisposeAsync();
            }
        } finally {
            _connectionLock.Release();
        }
    }

    public async Task Dispatch(MessageReceivedEventArgs args) {
        await hubContext.Clients.Group(args.ConversationId.ToString()).SendAsync(MessageReceivedEventName, args);
    }

    public async Task Dispatch(MessageDeletedEventArgs args) {
        await hubContext.Clients.Group(args.ConversationId.ToString()).SendAsync(MessageDeletedEventName, args);
    }

    public async Task Dispatch(MessageEditedEventArgs args) {
        await hubContext.Clients.Group(args.ConversationId.ToString()).SendAsync(MessageEditedEventName, args);
    }

    public async ValueTask DisposeAsync() {
        foreach ((_, HubConnection connection) in _hubConnections) {
            await connection.DisposeAsync();
        }

        _connectionLock.Dispose();
    }
}