using Conflux.Core;

namespace Conflux.Services.Abstracts;

public interface ICallRoomsService {
    CallRoom? GetCallRoom(Guid id);
    CallRoom CreateCallRoom(string senderId, string receiverId);
}