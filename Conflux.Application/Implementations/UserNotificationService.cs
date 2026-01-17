using Conflux.Application.Abstracts;
using Conflux.Domain.Events;

namespace Conflux.Application.Implementations;

public sealed class UserNotificationService : IUserNotificationService {
    public event Action<CommunityBannedEventArgs>? OnCommunityBanned;

    public void Dispatch(CommunityBannedEventArgs args) {
        OnCommunityBanned?.Invoke(args);
    }
}