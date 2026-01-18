using System.Text.Json.Serialization;

namespace Conflux.Core;

public sealed class CallRoom {
    public Guid Id { get; init; }
    public string InitiatorUserId { get; init; }
    public string ReceiverUserId { get; init; }
    
    public CallRoom(string initiatorUserId, string receiverUserId) {
        Id = Guid.CreateVersion7();
        InitiatorUserId = initiatorUserId;
        ReceiverUserId = receiverUserId;
    }
}