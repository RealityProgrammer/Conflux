using Conflux.Application.Abstracts;
using Conflux.Domain.Events;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace Conflux.Services.Implementations;

public sealed class UserNotificationService(
    IHubContext hubContext,
    NavigationManager navigationManager,
    IHttpContextAccessor httpContextAccessor
) : IUserNotificationService {
    public const string CommunityBannedEventName = "CommunityBanned";
    
    public event Action<CommunityBannedEventArgs>? OnCommunityBanned;
    
    private HubConnection? _hubConnection;

    public async Task Connect() {
        if (_hubConnection != null) return;
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri("/hub/user-notification"), options => {
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

        _hubConnection.On<CommunityBannedEventArgs>(CommunityBannedEventName, args => {
            OnCommunityBanned?.Invoke(args);
        });
        
        await _hubConnection.StartAsync();
    }

    public async Task Disconnect() {
        if (_hubConnection != null) {
            await _hubConnection.DisposeAsync();
        }
    }

    public async Task Dispatch(CommunityBannedEventArgs args) {
        var user = hubContext.Clients.User(args.UserId);
        
        await user.SendAsync(CommunityBannedEventName, args);
    }
}