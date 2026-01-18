using Conflux.Core;
using Conflux.Domain.Events;
using Conflux.Services.Abstracts;
using Conflux.Services.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace Conflux.Services.Implementations;

public sealed class UserCallService(
    ILogger<UserCallService> logger,
    NavigationManager navigationManager,
    IHttpContextAccessor httpContextAccessor,
    IHubContext<WebRTCSignalingHub> hubContext,
    ICallRoomsService callRoomsService
) : IUserCallService, IAsyncDisposable {
    public event Action? OnCallInitialized;
    public event Action<CallRoom>? OnOfferReceived;
    public event Action<CallRoom, string>? OnAnswerReceived;
    public event Action<CallRoom, string>? OnIceCandidateReceived;

    private readonly List<CallRoom> _rooms = [];
    public IReadOnlyList<CallRoom> Rooms => _rooms;
    
    private HubConnection? _hubConnection;

    public async Task Connect() {
        if (_hubConnection != null) return;
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri("/hub/webrtc"), options => {
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

        _hubConnection.On<Guid>("offer", roomId => {
            if (callRoomsService.GetCallRoom(roomId) is { } room) {
                _rooms.Add(room);
                OnOfferReceived?.Invoke(room);
            }
        });

        _hubConnection.On<Guid, string>("answer", (roomId, answer) => {
            if (callRoomsService.GetCallRoom(roomId) is { } room) {
                OnAnswerReceived?.Invoke(room, answer);
            }
        });
        
        _hubConnection.On<Guid, string>("ice-candidate", (roomId, iceCandidate) => {
            if (callRoomsService.GetCallRoom(roomId) is { } room) {
                OnIceCandidateReceived?.Invoke(room, iceCandidate);
            }
        });

        await _hubConnection.StartAsync();
    }

    public async Task Disconnect() {
        if (_hubConnection != null) {
            await _hubConnection.DisposeAsync();
        }
    }
    
    public Task<bool> InitializeDirectCall(string fromUserId, string receiverUserId) {
        _rooms.Add(callRoomsService.CreateCallRoom(fromUserId, receiverUserId));
        OnCallInitialized?.Invoke();
        
        return Task.FromResult(true);
    }

    public async Task SendOffer(CallRoom room, string senderId, string offer) {
        if (!_rooms.Contains(room)) {
            return;
        }

        room.OfferDescription = offer;
        await hubContext.Clients.User(room.ReceiverUserId).SendAsync("offer", room.Id);
    }

    public async Task SendAnswer(CallRoom room, string senderId, string answer) {
        if (!_rooms.Contains(room)) {
            return;
        }
        
        await hubContext.Clients.User(room.InitiatorUserId).SendAsync("answer", room.Id, answer);
    }

    public async Task SendIceCandidate(CallRoom room, string receiverId, string candidate) {
        if (!_rooms.Contains(room)) {
            return;
        }
        
        await hubContext.Clients.User(receiverId).SendAsync("ice-candidate", room.Id, candidate);
    }

    public async ValueTask DisposeAsync() {
        await Disconnect();
    }
}