namespace Conflux.Web.Core;

public sealed class CallRoom {
    public Guid Id { get; init; }
    public Guid InitiatorUserId { get; init; }
    public Guid ReceiverUserId { get; init; }
    public string? OfferDescription { get; set; }
    
    public CallRoom(Guid initiatorUserId, Guid receiverUserId) {
        Id = Guid.CreateVersion7();
        InitiatorUserId = initiatorUserId;
        ReceiverUserId = receiverUserId;
    }
}