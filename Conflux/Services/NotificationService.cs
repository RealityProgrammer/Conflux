using Conflux.Services.Abstracts;
using Conflux.Services.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace Conflux.Services;

public sealed class NotificationService(
    ILogger<NotificationService> logger,
    IHubContext<NotificationHub> hubContext,
    NavigationManager navigationManager,
    IHttpContextAccessor httpContextAccessor
) : INotificationService {

    private HubConnection? _hubConnection;

    public async Task InitializeConnection(CancellationToken cancellationToken) {
        if (_hubConnection != null) return;
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri("/hub/notification"), options => {
                var cookies = httpContextAccessor.HttpContext!.Request.Cookies.ToDictionary();
                
                options.UseDefaultCredentials = true;
                
                var cookieContainer = cookies.Count != 0 ? new(cookies.Count) : new CookieContainer();
                foreach (var cookie in cookies)
                    cookieContainer.Add(new Cookie(
                        cookie.Key,
                        WebUtility.UrlEncode(cookie.Value),
                        path: "/",
                        domain: navigationManager.ToAbsoluteUri("/").Host));
                options.Cookies = cookieContainer;

                foreach (var header in cookies)
                    options.Headers.Add(header.Key, header.Value);

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

        _hubConnection.On<string>("ReceiveFriendRequest", senderId => {
            logger.LogInformation("Received friend request from {id}", senderId);
        });
        
        await _hubConnection.StartAsync(cancellationToken);
    }

    public async Task NotifyFriendRequestAsync(NotifyFriendRequestModel model) {
        var user = hubContext.Clients.User(model.ReceiverId);
        
        await user.SendAsync("ReceiveFriendRequest", model.SenderId);
    }

    public async ValueTask DisposeAsync() {
        if (_hubConnection != null) {
            await _hubConnection.DisposeAsync();
        }
    }
}