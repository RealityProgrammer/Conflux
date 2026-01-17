namespace Conflux.Core;

public class DirectCallRoom : CallRoom {
    public string OfferUserId { get; init; }
    public string ReceiverUserId { get; init; }

    public DirectCallRoom(string offerUserId, string receiverUserId) {
        Id = Guid.CreateVersion7();
        OfferUserId = offerUserId;
        ReceiverUserId = receiverUserId;
    }
}