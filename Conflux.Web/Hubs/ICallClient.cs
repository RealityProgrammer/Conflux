using Conflux.Web.Services.Abstracts;

namespace Conflux.Web.Hubs;

public interface ICallClient {
    Task UserHangUp(CallUserHangUpEventArgs args);
    Task CallAccepted(Guid callId);

    Task Offer(Guid callId, string offer);
    Task Answer(Guid callId, string answer);
    Task IceCandidate(Guid callId, string iceCandidate);
}