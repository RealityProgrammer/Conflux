namespace Conflux.Services.Abstracts;

public record NotifyFriendRequestModel(string SenderId, string ReceiverId);

public interface INotificationService {
    Task NotifyFriendRequestAsync(NotifyFriendRequestModel model);
}