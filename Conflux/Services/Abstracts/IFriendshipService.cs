using Conflux.Database.Entities;

namespace Conflux.Services.Abstracts;

public interface IFriendshipService {
    Task<SendingStatus> SendFriendRequest(string senderId, string receiverId);
    Task<bool> CancelFriendRequest(string senderId, string receiverId);
    Task<bool> RejectFriendRequest(string senderId, string receiverId);
    Task<bool> AcceptFriendRequest(string senderId, string receiverId);

    public enum SendingStatus {
        Success,
        IncomingPending,
        OutcomingPending,
        Failed,
        Friended,
    }
}