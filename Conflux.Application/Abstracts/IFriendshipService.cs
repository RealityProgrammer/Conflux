using Conflux.Domain.Entities;

namespace Conflux.Application.Abstracts;

public interface IFriendshipService {
    Task<SendingResult> SendFriendRequestAsync(Guid senderId, Guid receiverId);
    Task<bool> CancelFriendRequestAsync(Guid friendRequestId);
    Task<bool> RejectFriendRequestAsync(Guid friendRequestId);
    Task<bool> AcceptFriendRequestAsync(Guid friendRequestId);
    Task<bool> UnfriendAsync(Guid friendRequestId);
    
    public enum SendingStatus {
        Success,
        IncomingPending,
        OutcomingPending,
        Failed,
        Friended,
    }
    
    public readonly record struct SendingResult(SendingStatus Status, Guid? RequestId);
}