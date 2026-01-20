using Conflux.Web.Core;

namespace Conflux.Web.Services.Abstracts;

public interface IUserCallService {
    event Action OnCallInitialized;
    event Action<CallRoom> OnOfferReceived;
    event Action<CallRoom, string> OnAnswerReceived;
    event Action<CallRoom, string> OnIceCandidateReceived;
    
    IReadOnlyList<CallRoom> Rooms { get; }

    Task Connect();
    Task Disconnect();
    
    Task<bool> InitializeDirectCall(string fromUserId, string receiverUserId);

    Task<IceServerConfiguration[]> CreateShortLivedIceServerConfiguration();
    
    Task SendOffer(CallRoom room, string senderId, string offer);
    Task SendAnswer(CallRoom room, string senderId, string answer);
    Task SendIceCandidate(CallRoom room, string receiverId, string candidate);
}