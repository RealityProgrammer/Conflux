using Conflux.Application.Abstracts;
using Conflux.Web.Core;
using Conflux.Web.Services.Abstracts;
using Conflux.Web.Services.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace Conflux.Web.Services.Implementations;

internal sealed class UserCallService : IUserCallService, IAsyncDisposable {
    public event Action? OnCallInitialized;
    public event Action<Guid, Guid>? OnUserHangUp;
    
    public event Action<CallRoom>? OnOfferReceived;
    public event Action<CallRoom, string>? OnAnswerReceived;
    public event Action<CallRoom, string>? OnIceCandidateReceived;

    private readonly List<CallRoom> _joinedRooms = [];
    public IReadOnlyList<CallRoom> JoinedRooms => _joinedRooms;
    
    private readonly ILogger<UserCallService> _logger;
    private readonly NavigationManager _navigationManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHubContext<CallingHub> _hubContext;
    private readonly ICallService _callServices;
    private readonly IWebUserNotificationService _userNotificationService;
    private readonly CloudflareTurnServerClient _cloudflareTurnServerClient;

    public UserCallService(
        ILogger<UserCallService> logger,
        NavigationManager navigationManager,
        IHttpContextAccessor httpContextAccessor,
        IHubContext<CallingHub> hubContext,
        ICallService callServices,
        IWebUserNotificationService userNotificationService,
        CloudflareTurnServerClient cloudflareTurnServerClient
    ) {
        _logger = logger;
        _navigationManager = navigationManager;
        _httpContextAccessor = httpContextAccessor;
        _hubContext = hubContext;
        _callServices = callServices;
        _userNotificationService = userNotificationService;
        _cloudflareTurnServerClient = cloudflareTurnServerClient;

        _userNotificationService.OnIncomingCall += OnIncomingCall;
    }

    private HubConnection CreateCallHubConnection() {
        var connection = new HubConnectionBuilder()
            .WithUrl(_navigationManager.ToAbsoluteUri("/hub/calling"), options => {
                var cookies = _httpContextAccessor.HttpContext!.Request.Cookies.ToDictionary();
                
                options.UseDefaultCredentials = true;
                
                var cookieContainer = cookies.Count != 0 ? new(cookies.Count) : new CookieContainer();

                foreach (var cookie in cookies) {
                    cookieContainer.Add(new Cookie(
                        cookie.Key,
                        WebUtility.UrlEncode(cookie.Value),
                        path: "/",
                        domain: _navigationManager.ToAbsoluteUri("/").Host));
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

        connection.On<Guid, Guid>("UserHangUp", (roomId, userId) => {
            OnUserHangUp?.Invoke(roomId, userId);
        });

        return connection;

        // connection.On<Guid>("offer", roomId => {
        //     if (_callRoomsService.GetCallRoom(roomId) is { } room) {
        //         _rooms.Add(room);
        //         OnOfferReceived?.Invoke(room);
        //     }
        // });
        //
        // _hubConnection.On<Guid, string>("answer", (roomId, answer) => {
        //     if (_callRoomsService.GetCallRoom(roomId) is { } room) {
        //         _logger.LogInformation("OnAnswerReceived?.Invoke()");
        //         OnAnswerReceived?.Invoke(room, answer);
        //     }
        // });
        //
        // _hubConnection.On<Guid, string>("ice-candidate", (roomId, iceCandidate) => {
        //     if (_callRoomsService.GetCallRoom(roomId) is { } room) {
        //         OnIceCandidateReceived?.Invoke(room, iceCandidate);
        //     }
        // });
        //
        // await _hubConnection.StartAsync();
    }

    // public async Task Disconnect() {
        // if (_hubConnection != null) {
        //     await _hubConnection.DisposeAsync();
        // }
    // }
    
    public async Task<bool> InitializeDirectCall(Guid fromUserId, Guid receiverUserId) {
        var callRoom = _callServices.CreateCallRoom(fromUserId, receiverUserId);
        _joinedRooms.Add(callRoom);
        OnCallInitialized?.Invoke();
        
        await _userNotificationService.Dispatch(new IncomingCallEventArgs(receiverUserId, callRoom.Id));

        return true;
    }

    public async Task HangUp(Guid callId, Guid userId) {
        if (_callServices.TryGetCallRoom(callId, out var room) && _joinedRooms.Remove(room)) {
            var otherUserId = room.InitiatorUserId == userId ? room.ReceiverUserId : room.InitiatorUserId;
            
            await _hubContext.Clients.User(otherUserId.ToString()).SendAsync("UserHangUp", room.Id, userId);
        }
    }

    public async Task SendOffer(CallRoom room, Guid senderId, string offer) {
        if (!_joinedRooms.Contains(room)) {
            return;
        }

        room.OfferDescription = offer;
        await _hubContext.Clients.User(room.ReceiverUserId.ToString()).SendAsync("offer", room.Id);
    }

    public async Task SendAnswer(CallRoom room, Guid senderId, string answer) {
        if (!_joinedRooms.Contains(room)) {
            return;
        }
        
        await _hubContext.Clients.User(room.InitiatorUserId.ToString()).SendAsync("answer", room.Id, answer);
    }

    public async Task SendIceCandidate(CallRoom room, Guid receiverId, string candidate) {
        if (!_joinedRooms.Contains(room)) {
            return;
        }
        
        await _hubContext.Clients.User(receiverId.ToString()).SendAsync("ice-candidate", room.Id, candidate);
    }

    public async Task<IceServerConfiguration[]> CreateShortLivedIceServerConfiguration() {
        return await _cloudflareTurnServerClient.GenerateIceServerConfigurations();
    }

    private void OnIncomingCall(IncomingCallEventArgs args) {
        if (_callServices.TryGetCallRoom(args.CallId, out var room) && room.ReceiverUserId == args.UserId) {
            _joinedRooms.Add(room);
        }
    }

    public async ValueTask DisposeAsync() {
        _userNotificationService.OnIncomingCall -= OnIncomingCall;
        // await Disconnect();
    }
}