using Conflux.Database.Entities;
using Microsoft.EntityFrameworkCore.Query;

namespace Conflux.Services;

public readonly struct PaginationRequest {
    public readonly Func<IQueryable<FriendRequest>, IQueryable<FriendRequest>>? Filter;
    public readonly Func<IQueryable<FriendRequest>, IQueryable<FriendRequest>>? Includes;
    public readonly Func<IQueryable<FriendRequest>, IQueryable<FriendRequest>> Order;

    public readonly int Offset;
    public readonly int Count;

    public PaginationRequest(Func<IQueryable<FriendRequest>, IQueryable<FriendRequest>>? filter, Func<IQueryable<FriendRequest>, IQueryable<FriendRequest>>? includes, Func<IQueryable<FriendRequest>, IQueryable<FriendRequest>>? order, int offset, int count) {
        ArgumentNullException.ThrowIfNull(order);
        
        Filter = filter;
        Includes = includes;
        Order = order;
        Offset = offset;
        Count = count;
    }
}