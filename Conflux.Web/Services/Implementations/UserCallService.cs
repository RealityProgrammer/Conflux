using Conflux.Web.Core;
using Conflux.Web.Services.Abstracts;
using Conflux.Web.Services.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace Conflux.Web.Services.Implementations;

internal sealed class UserCallService : IUserCallService, IAsyncDisposable {
    public event Action? OnCallJoined;
    public event Action<Guid>? OnCallLeft;
    public event Action<CallUserHangUpEventArgs>? OnUserHangUp;
    public event Action<CallRoom>? OnCallAccepted;
    
    public event Action<CallRoom, string>? OnOfferReceived;
    public event Action<CallRoom, string>? OnAnswerReceived;
    public event Action<CallRoom, string>? OnIceCandidateReceived;

    private readonly List<CallRoom> _joinedRooms = [];
    public IReadOnlyList<CallRoom> JoinedRooms => _joinedRooms;

    private readonly Dictionary<Guid, HubConnection> _callConnections = [];
    
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

    private async Task<HubConnection> CreateCallHubConnection(Guid callId) {
        var connection = new HubConnectionBuilder()
            .WithUrl(_navigationManager.ToAbsoluteUri($"/hub/calling?CallId={callId.ToString()}"), options => {
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

        connection.On<CallUserHangUpEventArgs>("UserHangUp", async args => {
            if (_callServices.TryGetCallRoom(args.CallId, out var callRoom)) {
                if (_joinedRooms.Remove(callRoom)) {
                    OnUserHangUp?.Invoke(args);

                    if (_callConnections.Remove(args.CallId, out var callConnection)) {
                        await callConnection.DisposeAsync();
                    }
                    
                    OnCallLeft?.Invoke(args.CallId);
                }
            }
        });

        connection.On<Guid>("CallAccepted", callId => {
            if (_callServices.GetCallRoom(callId) is { } callRoom) {
                OnCallAccepted?.Invoke(callRoom);
            }
        });

        connection.On<Guid, string>("offer", (callId, offer) => {
            if (_callServices.GetCallRoom(callId) is { } room && _joinedRooms.Contains(room)) {
                OnOfferReceived?.Invoke(room, offer);
            }
        });
        
        connection.On<Guid, string>("answer", (callId, answer) => {
            if (_callServices.GetCallRoom(callId) is { } room) {
                OnAnswerReceived?.Invoke(room, answer);
            }
        });
        
        connection.On<Guid, string>("ice-candidate", (callId, iceCandidate) => {
            if (_callServices.GetCallRoom(callId) is { } room) {
                OnIceCandidateReceived?.Invoke(room, iceCandidate);
            }
        });

        await connection.StartAsync();

        return connection;
    }
    
    public async Task<bool> InitializeDirectCall(Guid fromUserId, Guid receiverUserId) {
        var callRoom = _callServices.CreateCallRoom(fromUserId, receiverUserId);
        _joinedRooms.Add(callRoom);
        
        await _userNotificationService.Dispatch(new IncomingCallEventArgs(receiverUserId, callRoom.Id));
        _callConnections.Add(callRoom.Id, await CreateCallHubConnection(callRoom.Id));

        OnCallJoined?.Invoke();

        return true;
    }

    public async Task LeaveCall(Guid callId, Guid userId) {
        if (_callServices.TryGetCallRoom(callId, out var room) && _joinedRooms.Remove(room)) {
            OnCallLeft?.Invoke(callId);
            
            var otherUserId = room.InitiatorUserId == userId ? room.ReceiverUserId : room.InitiatorUserId;
            
            await _hubContext.Clients.User(otherUserId.ToString()).SendAsync("UserHangUp", new CallUserHangUpEventArgs(callId, userId));
            
            if (_callConnections.Remove(callId, out var hubConnection)) {
                await hubConnection.DisposeAsync();
            }
        }
    }

    public async Task<bool> AcceptIncomingCall(Guid callId, Guid receiverUserId) {
        if (_callServices.TryGetCallRoom(callId, out var room) && room.ReceiverUserId == receiverUserId && _joinedRooms.Contains(room)) {
            room.State = CallRoomState.Calling;

            await _hubContext.Clients.Group(callId.ToString()).SendAsync("CallAccepted", callId);
            
            // OnCallLeft?.Invoke(callId);
            //
            // var otherUserId = room.InitiatorUserId == userId ? room.ReceiverUserId : room.InitiatorUserId;
            //
            // await _hubContext.Clients.User(otherUserId.ToString()).SendAsync("UserHangUp", new CallUserHangUpEventArgs(callId, userId));
            //
            // if (_callConnections.Remove(callId, out var hubConnection)) {
            //     await hubConnection.DisposeAsync();
            // }

            return true;
        }

        return false;
    }

    public async Task SendOffer(CallRoom room, Guid senderId, string offer) {
        if (!_joinedRooms.Contains(room)) {
            return;
        }

        _logger.LogInformation("Send Offer.");
        await _hubContext.Clients.User(room.ReceiverUserId.ToString()).SendAsync("offer", room.Id, offer);
    }

    public async Task SendAnswer(CallRoom room, Guid senderId, string answer) {
        if (!_joinedRooms.Contains(room)) {
            return;
        }
        
        await _hubContext.Clients.User(room.InitiatorUserId.ToString()).SendAsync("answer", room.Id, answer);
    }

    public async Task SendIceCandidate(CallRoom room, Guid targetUserId, string candidate) {
        if (!_joinedRooms.Contains(room)) {
            return;
        }
        
        await _hubContext.Clients.User(targetUserId.ToString()).SendAsync("ice-candidate", room.Id, candidate);
    }

    public async Task<IceServerConfiguration[]> CreateShortLivedIceServerConfiguration() {
        return await _cloudflareTurnServerClient.GenerateIceServerConfigurations();
    }

    private async Task OnIncomingCall(IncomingCallEventArgs args) {
        if (_callServices.TryGetCallRoom(args.CallId, out var room) && room.ReceiverUserId == args.UserId) {
            _joinedRooms.Add(room);
            _callConnections.Add(args.CallId, await CreateCallHubConnection(args.CallId));
        }
    }

    public async ValueTask DisposeAsync() {
        foreach ((_, var hubConnection) in _callConnections) {
            await hubConnection.DisposeAsync();
        }

        _callConnections.Clear();
        
        _userNotificationService.OnIncomingCall -= OnIncomingCall;
        // await Disconnect();
    }
}