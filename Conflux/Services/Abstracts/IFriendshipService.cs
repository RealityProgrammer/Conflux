using Conflux.Database.Entities;

namespace Conflux.Services.Abstracts;

public interface IFriendshipService {
    Task<bool> SendFriendRequest(string senderId, string receiverId);
    Task<bool> CancelFriendRequest(string senderId, string receiverId);
    Task<bool> RejectFriendRequest(string senderId, string receiverId);
    Task<bool> AcceptFriendRequest(string senderId, string receiverId);

    Task<Pageable<FriendRequest>> GetOutcomingPendingFriendRequests(string userId, QueryRequest request);
    Task<Pageable<FriendRequest>> GetIncomingPendingFriendRequests(string userId, QueryRequest request);
}