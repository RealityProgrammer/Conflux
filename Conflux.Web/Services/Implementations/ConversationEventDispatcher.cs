using Conflux.Application.Abstracts;
using Conflux.Domain.Events;
using Conflux.Web.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace Conflux.Web.Services.Implementations;

public sealed class ConversationEventDispatcher(
    IHubContext<ConversationHub, IConversationClient> hubContext, 
    IHttpContextAccessor httpContextAccessor, 
    NavigationManager navigationManager
) : IConversationEventDispatcher, IAsyncDisposable {
    public event Action<MessageReceivedEventArgs>? OnMessageReceived;
    public event Action<MessageDeletedEventArgs>? OnMessageDeleted;
    public event Action<MessageEditedEventArgs>? OnMessageEdited;
    
    private readonly Dictionary<Guid, HubConnection> _hubConnections = [];
    
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

                options.HttpMessageHandlerFactory = _ => {
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
        if (_hubConnections.ContainsKey(conversationId)) return;

        var connection = CreateHubConnection(conversationId);

        connection.On<MessageReceivedEventArgs>(nameof(IConversationClient.MessageReceived), args => {
            OnMessageReceived?.Invoke(args);
        });
        
        connection.On<MessageDeletedEventArgs>(nameof(IConversationClient.MessageDeleted), args => {
            OnMessageDeleted?.Invoke(args);
        });
        
        connection.On<MessageEditedEventArgs>(nameof(IConversationClient.MessageEdited), args => {
            OnMessageEdited?.Invoke(args);
        });

        await connection.StartAsync();

        _hubConnections.Add(conversationId, connection);
    }

    public async Task Disconnect(Guid conversationId) {
        if (_hubConnections.Remove(conversationId, out var connection)) {
            await connection.DisposeAsync();
        }
    }

    public async Task Dispatch(MessageReceivedEventArgs args) {
        await hubContext.Clients.Group(args.ConversationId.ToString()).MessageReceived(args);
    }

    public async Task Dispatch(MessageDeletedEventArgs args) {
        await hubContext.Clients.Group(args.ConversationId.ToString()).MessageDeleted(args);
    }

    public async Task Dispatch(MessageEditedEventArgs args) {
        await hubContext.Clients.Group(args.ConversationId.ToString()).MessageEdited(args);
    }

    public async ValueTask DisposeAsync() {
        foreach ((_, HubConnection connection) in _hubConnections) {
            await connection.DisposeAsync();
        }
    }
}