using Conflux.Database;
using Conflux.Database.Entities;

namespace Conflux.Services.Abstracts;

public interface IFriendshipService {
    Task<bool> SendFriendRequest(string senderId, string receiverId);
    Task<bool> CancelFriendRequest(string senderId, string receiverId);
    
    Task<FriendRequestStatus?> GetRequestStatus(string senderId, string receiverId);
}