using Conflux.Application.Abstracts;
using Conflux.Domain.Enums;
using Conflux.Domain.Events;
using Conflux.Web.Hubs;
using Conflux.Web.Services.Abstracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace Conflux.Web.Services.Implementations;

public sealed class UserNotificationService(
    IHubContext<UserNotificationHub, IUserClient> hubContext,
    NavigationManager navigationManager,
    IHttpContextAccessor httpContextAccessor
) : IWebUserNotificationService, IAsyncDisposable {
    public event Action<CommunityBannedEventArgs>? OnCommunityBanned;
    public event Func<IncomingCallEventArgs, Task>? OnIncomingCall;
    public event Action<IncomingDirectMessageEventArgs>? OnIncomingDirectMessage;

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

        _hubConnection.On<CommunityBannedEventArgs>(nameof(IUserClient.CommunityBanned), args => {
            OnCommunityBanned?.Invoke(args);
        });

        _hubConnection.On<IncomingDirectMessageEventArgs>(nameof(IUserClient.IncomingDirectMessage), args => {
            OnIncomingDirectMessage?.Invoke(args);
        });

        _hubConnection.On<IncomingCallEventArgs>(nameof(IUserClient.IncomingCall), async args => {
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
        await hubContext.Clients.User(args.UserId.ToString()).CommunityBanned(args);
    }

    public async Task Dispatch(IncomingDirectMessageEventArgs args) {
        await hubContext.Clients.User(args.TargetUserId.ToString()).IncomingDirectMessage(args);
    }

    public async Task Dispatch(IncomingCallEventArgs args) {
        await hubContext.Clients.User(args.UserId.ToString()).IncomingCall(args);
    }

    public async ValueTask DisposeAsync() {
        await Disconnect();
    }
}