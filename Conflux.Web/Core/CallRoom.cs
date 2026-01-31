namespace Conflux.Web.Core;

public sealed class CallRoom {
    public Guid Id { get; init; }
    public Guid InitiatorUserId { get; init; }
    public Guid ReceiverUserId { get; init; }
    public CallRoomState State { get; internal set; }
    
    public CallRoom(Guid initiatorUserId, Guid receiverUserId) {
        Id = Guid.CreateVersion7();
        InitiatorUserId = initiatorUserId;
        ReceiverUserId = receiverUserId;
    }
}