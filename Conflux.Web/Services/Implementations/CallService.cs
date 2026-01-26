using Conflux.Web.Core;
using Conflux.Web.Services.Abstracts;
using System.Diagnostics.CodeAnalysis;

namespace Conflux.Web.Services.Implementations;

public sealed class CallService : ICallService {
    private readonly Dictionary<Guid, CallRoom> _callRooms = [];
    private readonly Lock _lock = new();

    public CallRoom? GetCallRoom(Guid id) {
        using (_lock.EnterScope()) {
            return _callRooms.GetValueOrDefault(id);
        }
    }
    
    public CallRoom CreateCallRoom(Guid senderId, Guid receiverId) {
        using (_lock.EnterScope()) {
            var room = new CallRoom(senderId, receiverId);
            _callRooms.Add(room.Id, room);
            return room;
        }
    }

    public bool TryGetCallRoom(Guid id, [NotNullWhen(true)] out CallRoom? room) {
        using (_lock.EnterScope()) {
            return _callRooms.TryGetValue(id, out room);
        }
    }
}