using Conflux.Services.Abstracts;
using Conflux.Services.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace Conflux.Services;

public sealed class NotificationService(
    IWebHostEnvironment environment,
    IHubContext<NotificationHub> hubContext,
    NavigationManager navigationManager,
    IHttpContextAccessor httpContextAccessor
) : INotificationService {
    
    public const string FriendRequestReceivedMethodName = "Social.ReceivedFriendRequest";
    public const string FriendRequestRejectedMethodName = "Social.RejectedFriendRequest";
    public const string FriendRequestCanceledMethodName = "Social.CanceledFriendRequest";
    public const string FriendRequestAcceptedMethodName = "Social.AcceptedFriendRequest";

    private HubConnection? _hubConnection;

    public event Action<FriendRequestReceivedEventArgs>? OnFriendRequestReceived;
    public event Action<FriendRequestRejectedEventArgs>? OnFriendRequestRejected;
    public event Action<FriendRequestCanceledEventArgs>? OnFriendRequestCanceled;
    public event Action<FriendRequestAcceptedEventArgs>? OnFriendRequestAccepted;

    public async Task InitializeConnection(CancellationToken cancellationToken) {
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
            .ConfigureLogging(logging => {
                if (environment.IsDevelopment()) {
                    logging.AddConsole();
                    
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                    logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
                }
            })
            .Build();

        _hubConnection.On<FriendRequestReceivedEventArgs>(FriendRequestReceivedMethodName, notif => {
            OnFriendRequestReceived?.Invoke(notif);
        });

        _hubConnection.On<FriendRequestRejectedEventArgs>(FriendRequestRejectedMethodName, notif => {
            OnFriendRequestRejected?.Invoke(notif);
        });

        _hubConnection.On<FriendRequestCanceledEventArgs>(FriendRequestCanceledMethodName, notif => {
            OnFriendRequestCanceled?.Invoke(notif);
        });
        
        _hubConnection.On<FriendRequestAcceptedEventArgs>(FriendRequestAcceptedMethodName, notif => {
            OnFriendRequestAccepted?.Invoke(notif);
        });
        
        await _hubConnection.StartAsync(cancellationToken);
    }

    public Task NotifyFriendRequestReceivedAsync(FriendRequestReceivedEventArgs eventArgs) {
        var user = hubContext.Clients.User(eventArgs.ReceiverId);
        
        return user.SendAsync(FriendRequestReceivedMethodName, eventArgs);
    }

    public Task NotifyFriendRequestCanceledAsync(FriendRequestCanceledEventArgs eventArgs) {
        var user = hubContext.Clients.User(eventArgs.ReceiverId);
        
        return user.SendAsync(FriendRequestCanceledMethodName, eventArgs);
    }

    public Task NotifyFriendRequestRejectedAsync(FriendRequestRejectedEventArgs eventArgs) {
        var user = hubContext.Clients.User(eventArgs.SenderId);

        return user.SendAsync(FriendRequestRejectedMethodName, eventArgs);
    }
    
    public Task NotifyFriendRequestAcceptedAsync(FriendRequestAcceptedEventArgs eventArgs) {
        Task senderSendTask = hubContext.Clients.User(eventArgs.SenderId).SendAsync(FriendRequestAcceptedMethodName, eventArgs);
        Task receiverSendTask = hubContext.Clients.User(eventArgs.ReceiverId).SendAsync(FriendRequestAcceptedMethodName, eventArgs);
        
        return Task.WhenAll(senderSendTask, receiverSendTask);
    }

    public async ValueTask DisposeAsync() {
        if (_hubConnection != null) {
            await _hubConnection.DisposeAsync();
        }
    }
}