using Conflux.Domain.Enums;
using Conflux.Domain.Events;

namespace Conflux.Application.Abstracts;

public interface IUserNotificationService {
    event Action<CommunityBannedEventArgs> OnCommunityBanned;
    event Action<IncomingDirectMessageEventArgs> OnIncomingDirectMessage;

    Task Connect();
    Task Disconnect();
    
    Task Dispatch(CommunityBannedEventArgs args);
    Task Dispatch(IncomingDirectMessageEventArgs args);
}