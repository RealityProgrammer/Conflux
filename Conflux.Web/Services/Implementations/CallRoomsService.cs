using Conflux.Web.Core;
using Conflux.Web.Services.Abstracts;

namespace Conflux.Web.Services.Implementations;

public sealed class CallRoomsService : ICallRoomsService {
    private readonly Dictionary<Guid, CallRoom> _callRooms = [];
    private readonly Lock _lock = new();

    public CallRoom? GetCallRoom(Guid id) {
        using (_lock.EnterScope()) {
            return _callRooms.GetValueOrDefault(id);
        }
    }
    
    public CallRoom CreateCallRoom(string senderId, string receiverId) {
        using (_lock.EnterScope()) {
            var room = new CallRoom(senderId, receiverId);
            _callRooms.Add(room.Id, room);
            return room;
        }
    }
}