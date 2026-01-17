using Conflux.Domain.Events;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Net;

namespace Conflux.Services.Implementations;

public sealed class WebRTCService(
    IJSRuntime javascriptRuntime, 
    NavigationManager navigationManager,
    IHttpContextAccessor httpContextAccessor
) : IWebRTCService, IAsyncDisposable {
    private HubConnection? _hubConnection;
    
    public event Action<string>? OnOfferReceived;
    public event Action<string>? OnAnswerReceived;
    public event Action<string>? OnIceCandidateReceived;
    public event Action<List<string>>? OnUsersListReceived;
    public event Action<string>? OnUserDisconnected;

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

        _hubConnection.On<string, string>("ReceiveOffer", (_, offer) => {
            OnOfferReceived?.Invoke(offer);
        });

        _hubConnection.On<string>("ReceiveAnswer", answer => {
            OnAnswerReceived?.Invoke(answer);
        });

        _hubConnection.On<string>("ReceiveIceCandidate", candidate => {
            OnIceCandidateReceived?.Invoke(candidate);
        });

        _hubConnection.On<List<string>>("Registered", users => {
            OnUsersListReceived?.Invoke(users);
        });

        _hubConnection.On<string>("UserDisconnected", userId => {
            OnUserDisconnected?.Invoke(userId);
        });

        await _hubConnection.StartAsync();
    }

    public async Task Disconnect() {
        if (_hubConnection != null) {
            await _hubConnection.DisposeAsync();
        }
    }

    public async Task SendOfferAsync(string targetUserId, string offer) {
        if (_hubConnection?.State == HubConnectionState.Connected) {
            await _hubConnection.SendAsync("SendOffer", targetUserId, offer);
        }
    }

    public async Task SendAnswerAsync(string targetUserId, string answer) {
        if (_hubConnection?.State == HubConnectionState.Connected) {
            await _hubConnection.SendAsync("SendAnswer", targetUserId, answer);
        }
    }

    public async Task SendIceCandidateAsync(string targetUserId, string candidate) {
        if (_hubConnection?.State == HubConnectionState.Connected) {
            await _hubConnection.SendAsync("SendIceCandidate", targetUserId, candidate);
        }
    }

    public async ValueTask DisposeAsync() {
        await Disconnect();
    }
}