namespace Conflux.Services.Abstracts;

public interface IWebRTCService {
    event Action<string>? OnOfferReceived;
    event Action<string>? OnAnswerReceived;
    event Action<string>? OnIceCandidateReceived;
    event Action<List<string>>? OnUsersListReceived;
    event Action<string>? OnUserDisconnected;
}