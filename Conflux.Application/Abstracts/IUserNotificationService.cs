using Conflux.Domain.Events;

namespace Conflux.Application.Abstracts;

public interface IUserNotificationService {
    event Action<CommunityBannedEventArgs> OnCommunityBanned;

    Task Connect(Guid userId);
    Task Disconnect();
    
    Task Dispatch(CommunityBannedEventArgs args);
}