using System.Diagnostics.CodeAnalysis;
using Conflux.Web.Core;

namespace Conflux.Web.Services.Abstracts;

public interface ICallService
{
    CallRoom? GetCallRoom(Guid id);
    CallRoom CreateCallRoom(Guid senderId, Guid receiverId);

    bool TryGetCallRoom(Guid id, [NotNullWhen(true)] out CallRoom? room);
    bool RemoveCallRoom(Guid id);
}