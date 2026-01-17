using Conflux.Domain.Events;

namespace Conflux.Application.Abstracts;

public interface IUserNotificationService {
    event Action<CommunityBannedEventArgs> OnCommunityBanned;

    void Dispatch(CommunityBannedEventArgs args);
}