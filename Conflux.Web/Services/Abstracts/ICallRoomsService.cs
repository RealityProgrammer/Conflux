using Conflux.Web.Core;

namespace Conflux.Web.Services.Abstracts;

public interface ICallRoomsService {
    CallRoom? GetCallRoom(Guid id);
    CallRoom CreateCallRoom(string senderId, string receiverId);
}