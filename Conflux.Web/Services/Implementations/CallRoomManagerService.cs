using Conflux.Core;
using Conflux.Services.Abstracts;

namespace Conflux.Services.Implementations;

public sealed class CallRoomManagerService : ICallRoomManagerService {
    public Task<DirectCallRoom> CreateDirectCallRoom(string fromUser, string toUser) {
        return Task.FromResult(new DirectCallRoom(fromUser, toUser));
    }
}