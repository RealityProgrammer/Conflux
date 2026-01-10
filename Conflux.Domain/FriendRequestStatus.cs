namespace Conflux.Domain;

public enum FriendRequestStatus {
    Pending,
    Accepted,
    Rejected,
    Canceled,
    Unfriended,
    
    // Special value, mostly used for code logic but can be used in the database in the future.
    None,
}