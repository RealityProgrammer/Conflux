using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Conflux.Services.Hubs;

[Authorize]
public sealed class WebRTCSignalingHub : Hub {
    private static readonly ConcurrentDictionary<string, UserRoom> _users = new();
    private static readonly ConcurrentDictionary<string, Room> _rooms = new();

    private class UserRoom {
        public string UserId { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
    }

    private class Room {
        public string RoomId { get; set; } = string.Empty;
        public List<string> Users { get; set; } = new();
    }

    public override async Task OnConnectedAsync() {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception) {
        var connectionId = Context.ConnectionId;
        var user = _users.Values.FirstOrDefault(u => u.ConnectionId == connectionId);

        if (user != null) {
            _users.TryRemove(user.UserId, out _);

            if (_rooms.TryGetValue(user.RoomId, out var room)) {
                room.Users.Remove(user.UserId);

                if (room.Users.Count == 0) {
                    _rooms.TryRemove(user.RoomId, out _);
                } else {
                    await Clients.Group(user.RoomId).SendAsync("userLeft", user.UserId);
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(string roomId, string userId) {
        var connectionId = Context.ConnectionId;
        var userRoom = new UserRoom {
            UserId = userId,
            RoomId = roomId,
            ConnectionId = connectionId
        };

        _users[userId] = userRoom;

        if (!_rooms.TryGetValue(roomId, out var room)) {
            room = new() {
                RoomId = roomId
            };
            _rooms[roomId] = room;
        }

        var existingUsers = room.Users.Where(u => u != userId).ToList();
        room.Users.Add(userId);

        await Groups.AddToGroupAsync(connectionId, roomId);
        await Clients.Caller.SendAsync("roomJoined", roomId, existingUsers);
        await Clients.OthersInGroup(roomId).SendAsync("newUserJoined", userId);
    }

    public async Task SendOffer(string roomId, string fromUserId, string toUserId, string offer) {
        if (_users.TryGetValue(toUserId, out var targetUser)) {
            await Clients.Client(targetUser.ConnectionId).SendAsync("receiveOffer", fromUserId, offer);
        }
    }

    public async Task SendAnswer(string roomId, string fromUserId, string toUserId, string answer) {
        if (_users.TryGetValue(toUserId, out var targetUser)) {
            await Clients.Client(targetUser.ConnectionId).SendAsync("receiveAnswer", fromUserId, answer);
        }
    }

    public async Task SendIceCandidate(string roomId, string fromUserId, string toUserId, string candidate) {
        if (_users.TryGetValue(toUserId, out var targetUser)) {
            await Clients.Client(targetUser.ConnectionId).SendAsync("receiveIceCandidate", fromUserId, candidate);
        }
    }

    public async Task LeaveRoom(string roomId, string userId) {
        if (_users.TryRemove(userId, out var user)) {
            if (_rooms.TryGetValue(roomId, out var room)) {
                room.Users.Remove(userId);

                await Groups.RemoveFromGroupAsync(user.ConnectionId, roomId);
                await Clients.OthersInGroup(roomId).SendAsync("userLeft", userId);

                if (room.Users.Count == 0) {
                    _rooms.TryRemove(roomId, out _);
                }
            }
        }
    }
}