using Conflux.Core;

namespace Conflux.Services.Abstracts;

public interface ICallRoomManagerService {
    Task<DirectCallRoom> CreateDirectCallRoom(string fromUser, string toUser);
}