using Conflux.Web.Core;

namespace Conflux.Web.Services.Abstracts;

public readonly record struct CallUserHangUpEventArgs(Guid CallId, Guid HangupUser);

public interface IUserCallService {
    event Action OnCallJoined;
    event Action<Guid> OnCallLeft;
    event Action<CallUserHangUpEventArgs> OnUserHangUp;
        
    event Action<CallRoom> OnOfferReceived;
    event Action<CallRoom, string> OnAnswerReceived;
    event Action<CallRoom, string> OnIceCandidateReceived;
    
    IReadOnlyList<CallRoom> JoinedRooms { get; }

    // Task Connect();
    // Task Disconnect();
    
    Task<bool> InitializeDirectCall(Guid fromUserId, Guid receiverUserId);
    Task LeaveCall(Guid callId, Guid userId);

    Task<IceServerConfiguration[]> CreateShortLivedIceServerConfiguration();
    
    Task SendOffer(CallRoom room, Guid senderId, string offer);
    Task SendAnswer(CallRoom room, Guid senderId, string answer);
    Task SendIceCandidate(CallRoom room, Guid receiverId, string candidate);
}