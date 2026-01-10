using Conflux.Domain.Events;
using Conflux.Services.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace Conflux.Services;

public sealed class FriendshipEventDispatcher(
    IHubContext<FriendshipHub> hubContext,
    NavigationManager navigationManager,
    IHttpContextAccessor httpContextAccessor
) : IFriendshipEventDispatcher {
    public const string FriendRequestReceivedMethodName = "Social.ReceivedFriendRequest";
    public const string FriendRequestRejectedMethodName = "Social.RejectedFriendRequest";
    public const string FriendRequestCanceledMethodName = "Social.CanceledFriendRequest";
    public const string FriendRequestAcceptedMethodName = "Social.AcceptedFriendRequest";
    public const string UnfriendedMethodName = "Social.Unfriended";

    private HubConnection? _hubConnection;

    public event Action<FriendRequestReceivedEventArgs>? OnFriendRequestReceived;
    public event Action<FriendRequestRejectedEventArgs>? OnFriendRequestRejected;
    public event Action<FriendRequestCanceledEventArgs>? OnFriendRequestCanceled;
    public event Action<FriendRequestAcceptedEventArgs>? OnFriendRequestAccepted;
    public event Action<UnfriendedEventArgs>? OnUnfriended;

    public async Task Connect(CancellationToken cancellationToken) {
        if (_hubConnection != null) return;
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri("/hub/notification"), options => {
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

        _hubConnection.On<FriendRequestReceivedEventArgs>(FriendRequestReceivedMethodName, args => {
            OnFriendRequestReceived?.Invoke(args);
        });

        _hubConnection.On<FriendRequestRejectedEventArgs>(FriendRequestRejectedMethodName, args => {
            OnFriendRequestRejected?.Invoke(args);
        });

        _hubConnection.On<FriendRequestCanceledEventArgs>(FriendRequestCanceledMethodName, args => {
            OnFriendRequestCanceled?.Invoke(args);
        });
        
        _hubConnection.On<FriendRequestAcceptedEventArgs>(FriendRequestAcceptedMethodName, args => {
            OnFriendRequestAccepted?.Invoke(args);
        });

        _hubConnection.On<UnfriendedEventArgs>(UnfriendedMethodName, args => {
            OnUnfriended?.Invoke(args);
        });
        
        await _hubConnection.StartAsync(cancellationToken);
    }

    public async Task Disconnect(CancellationToken cancellationToken) {
        if (_hubConnection != null) {
            await _hubConnection.DisposeAsync();
        }
    }

    public async Task NotifyFriendRequestReceivedAsync(FriendRequestReceivedEventArgs args) {
        var user = hubContext.Clients.User(args.ReceiverId);
        
        await user.SendAsync(FriendRequestReceivedMethodName, args);
    }

    public async Task NotifyFriendRequestCanceledAsync(FriendRequestCanceledEventArgs args) {
        var user = hubContext.Clients.User(args.ReceiverId);
        
        await user.SendAsync(FriendRequestCanceledMethodName, args);
    }

    public async Task NotifyFriendRequestRejectedAsync(FriendRequestRejectedEventArgs args) {
        var user = hubContext.Clients.User(args.SenderId);

        await user.SendAsync(FriendRequestRejectedMethodName, args);
    }
    
    public async Task NotifyFriendRequestAcceptedAsync(FriendRequestAcceptedEventArgs args) {
        await hubContext.Clients.User(args.SenderId).SendAsync(FriendRequestAcceptedMethodName, args);
    }

    public async Task NotifyUnfriendedAsync(UnfriendedEventArgs args) {
        await hubContext.Clients.User(args.User1).SendAsync(UnfriendedMethodName, args);
        await hubContext.Clients.User(args.User2).SendAsync(UnfriendedMethodName, args);
    }

    public async ValueTask DisposeAsync() {
        await Disconnect(CancellationToken.None);
    }
}