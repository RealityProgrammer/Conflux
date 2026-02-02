using Conflux.Application.Abstracts;
using Conflux.Domain.Events;
using Conflux.Web.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace Conflux.Web.Services.Implementations;

public sealed class FriendshipEventDispatcher(
    IHubContext<FriendshipHub, IFriendshipClient> hubContext,
    NavigationManager navigationManager,
    IHttpContextAccessor httpContextAccessor
) : IFriendshipEventDispatcher {
    private HubConnection? _hubConnection;

    public event Action<FriendRequestReceivedEventArgs>? OnFriendRequestReceived;
    public event Action<FriendRequestRejectedEventArgs>? OnFriendRequestRejected;
    public event Action<FriendRequestCanceledEventArgs>? OnFriendRequestCanceled;
    public event Action<FriendRequestAcceptedEventArgs>? OnFriendRequestAccepted;
    public event Action<UnfriendedEventArgs>? OnUnfriended;

    public async Task Connect() {
        if (_hubConnection != null) return;
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri("/hub/friendship"), options => {
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

        _hubConnection.On<FriendRequestReceivedEventArgs>(nameof(IFriendshipClient.FriendRequestReceived), args => {
            OnFriendRequestReceived?.Invoke(args);
        });

        _hubConnection.On<FriendRequestRejectedEventArgs>(nameof(IFriendshipClient.FriendRequestRejected), args => {
            OnFriendRequestRejected?.Invoke(args);
        });

        _hubConnection.On<FriendRequestCanceledEventArgs>(nameof(IFriendshipClient.FriendRequestCanceled), args => {
            OnFriendRequestCanceled?.Invoke(args);
        });
        
        _hubConnection.On<FriendRequestAcceptedEventArgs>(nameof(IFriendshipClient.FriendRequestAccepted), args => {
            OnFriendRequestAccepted?.Invoke(args);
        });

        _hubConnection.On<UnfriendedEventArgs>(nameof(IFriendshipClient.Unfriended), args => {
            OnUnfriended?.Invoke(args);
        });
        
        await _hubConnection.StartAsync();
    }

    public async Task Disconnect() {
        if (_hubConnection != null) {
            await _hubConnection.DisposeAsync();
        }
    }

    public async Task NotifyFriendRequestReceivedAsync(FriendRequestReceivedEventArgs args) {
        await hubContext.Clients.User(args.ReceiverId.ToString()).FriendRequestReceived(args);
    }

    public async Task NotifyFriendRequestCanceledAsync(FriendRequestCanceledEventArgs args) {
        await hubContext.Clients.User(args.ReceiverId.ToString()).FriendRequestCanceled(args);
    }

    public async Task NotifyFriendRequestRejectedAsync(FriendRequestRejectedEventArgs args) {
        await hubContext.Clients.User(args.SenderId.ToString()).FriendRequestRejected(args);
    }
    
    public async Task NotifyFriendRequestAcceptedAsync(FriendRequestAcceptedEventArgs args) {
        await hubContext.Clients.User(args.SenderId.ToString()).FriendRequestAccepted(args);
    }

    public async Task NotifyUnfriendedAsync(UnfriendedEventArgs args) {
        await hubContext.Clients.Users([args.User1.ToString(), args.User2.ToString()]).Unfriended(args);
    }

    public async ValueTask DisposeAsync() {
        await Disconnect();
    }
}