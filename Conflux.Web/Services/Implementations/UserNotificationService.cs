using Conflux.Application.Abstracts;
using Conflux.Domain.Events;
using Conflux.Web.Services.Abstracts;
using Conflux.Web.Services.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace Conflux.Web.Services.Implementations;

public sealed class UserNotificationService(
    IHubContext<UserNotificationHub> hubContext,
    NavigationManager navigationManager,
    IHttpContextAccessor httpContextAccessor
) : IWebUserNotificationService, IAsyncDisposable {
    public const string CommunityBannedEventName = "CommunityBanned";
    public const string IncomingCallEventName = "IncomingCall";
    
    public event Action<CommunityBannedEventArgs>? OnCommunityBanned;
    public event Func<IncomingCallEventArgs, Task>? OnIncomingCall;

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
            .ConfigureLogging(logging => {
                logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
            })
            .Build();

        _hubConnection.On<CommunityBannedEventArgs>(CommunityBannedEventName, args => {
            OnCommunityBanned?.Invoke(args);
        });

        _hubConnection.On<IncomingCallEventArgs>(IncomingCallEventName, async args => {
            if (OnIncomingCall != null) {
                await OnIncomingCall.Invoke(args);
            }
        });

        await _hubConnection.StartAsync();
    }

    public async Task Disconnect() {
        if (_hubConnection != null) {
            await _hubConnection.DisposeAsync();
        }
    }

    public async Task Dispatch(CommunityBannedEventArgs args) {
        var user = hubContext.Clients.User(args.UserId.ToString());
        
        await user.SendAsync(CommunityBannedEventName, args);
    }

    public async Task Dispatch(IncomingCallEventArgs args) {
        var user = hubContext.Clients.User(args.UserId.ToString());
        
        await user.SendAsync(IncomingCallEventName, args);
    }

    public async ValueTask DisposeAsync() {
        await Disconnect();
    }
}