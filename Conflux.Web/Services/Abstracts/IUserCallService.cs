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
    
    Task<bool> InitializeDirectCall(Guid fromUserId, Guid receiverUserId);

    Task<IceServerConfiguration[]> CreateShortLivedIceServerConfiguration();
    
    Task SendOffer(CallRoom room, Guid senderId, string offer);
    Task SendAnswer(CallRoom room, Guid senderId, string answer);
    Task SendIceCandidate(CallRoom room, Guid receiverId, string candidate);
}