namespace Conflux.Services.Abstracts;

public record NotifyFriendRequestModel(string SenderId, string ReceiverId);

public interface INotificationService : IAsyncDisposable {
    Task InitializeConnection(CancellationToken cancellationToken);
    Task NotifyFriendRequestAsync(NotifyFriendRequestModel model);
}