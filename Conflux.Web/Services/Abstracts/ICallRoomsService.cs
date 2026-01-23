using Conflux.Web.Core;

namespace Conflux.Web.Services.Abstracts;

public interface ICallRoomsService {
    CallRoom? GetCallRoom(Guid id);
    CallRoom CreateCallRoom(Guid senderId, Guid receiverId);
}