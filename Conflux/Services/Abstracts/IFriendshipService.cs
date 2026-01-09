using Conflux.Database.Entities;

namespace Conflux.Services.Abstracts;

public interface IFriendshipService {
    Task<SendingStatus> SendFriendRequestAsync(string senderId, string receiverId);
    Task<bool> CancelFriendRequestAsync(string senderId, string receiverId);
    Task<bool> RejectFriendRequestAsync(string senderId, string receiverId);
    Task<bool> AcceptFriendRequestAsync(string senderId, string receiverId);
    Task<bool> UnfriendAsync(string user1, string user2);
    Task<bool> UnfriendAsync(Guid friendRequestId);

    public enum SendingStatus {
        Success,
        IncomingPending,
        OutcomingPending,
        Failed,
        Friended,
    }
}