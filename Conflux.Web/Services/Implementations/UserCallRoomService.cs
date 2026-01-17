using Conflux.Core;
using Conflux.Services.Abstracts;

namespace Conflux.Services.Implementations;

public sealed class UserCallRoomService : IUserCallRoomService {
    public event Action? OnCallRoomsChanged;

    private readonly List<CallRoom> _joinedRooms = [];
    public IReadOnlyList<CallRoom> JoinedRooms => _joinedRooms;
    
    public Task<bool> JoinDirectCallRoom(DirectCallRoom directCallRoom) {
        _joinedRooms.Add(directCallRoom);
        OnCallRoomsChanged?.Invoke();
        
        return Task.FromResult(true);
    }
}