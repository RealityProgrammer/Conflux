using Conflux.Core;

namespace Conflux.Services.Abstracts;

public interface IUserCallService {
    event Action OnCallInitialized;
    event Action<CallRoom> OnOfferReceived;
    event Action<CallRoom, string> OnAnswerReceived;
    event Action<CallRoom, string> OnIceCandidateReceived;
    
    IReadOnlyList<CallRoom> Rooms { get; }

    Task Connect();
    Task Disconnect();
    
    Task<bool> InitializeDirectCall(string fromUserId, string receiverUserId);

    Task SendOffer(CallRoom room, string senderId, string offer);
    Task SendAnswer(CallRoom room, string senderId, string answer);
    Task SendIceCandidate(CallRoom room, string receiverId, string candidate);
}