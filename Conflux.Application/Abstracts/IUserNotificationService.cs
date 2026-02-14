using Conflux.Domain.Events;

namespace Conflux.Application.Abstracts;

public interface IUserNotificationService {
    event Action<SystemWarnedEventArgs> OnSystemWarned;
    event Action<SystemBannedEventArgs> OnSystemBanned;

    event Action<CommunityWarnedEventArgs> OnCommunityWarned;
    event Action<CommunityBannedEventArgs> OnCommunityBanned;
    
    event Action<IncomingDirectMessageEventArgs> OnIncomingDirectMessage;

    Task Connect();
    Task Disconnect();

    Task Dispatch(SystemWarnedEventArgs args);
    Task Dispatch(SystemBannedEventArgs args);
    Task Dispatch(CommunityWarnedEventArgs args);
    Task Dispatch(CommunityBannedEventArgs args);
    Task Dispatch(IncomingDirectMessageEventArgs args);
}