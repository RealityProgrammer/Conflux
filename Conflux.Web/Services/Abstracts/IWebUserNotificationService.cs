using Conflux.Application.Abstracts;

namespace Conflux.Web.Services.Abstracts;

public readonly record struct IncomingCallEventArgs(Guid UserId, Guid CallId);

public interface IWebUserNotificationService : IUserNotificationService {
    event Func<IncomingCallEventArgs, Task> OnIncomingCall;
    
    Task Dispatch(IncomingCallEventArgs args);
}