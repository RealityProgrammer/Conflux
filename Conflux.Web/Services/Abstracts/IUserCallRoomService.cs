using Conflux.Core;

namespace Conflux.Services.Abstracts;

public interface IUserCallRoomService {
    event Action OnCallRoomsChanged;
    
    IReadOnlyList<CallRoom> JoinedRooms { get; }
    
    Task<bool> JoinDirectCallRoom(DirectCallRoom directCallRoom);
}