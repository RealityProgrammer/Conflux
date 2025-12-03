namespace Conflux.Services;

public readonly struct QueryRequest<TType, TProjection> {
    public readonly Func<IQueryable<TType>, IQueryable<TType>>? Filter;
    public readonly Func<IQueryable<TType>, IQueryable<TType>>? Includes;
    public readonly Func<IQueryable<TType>, IQueryable<TType>> Order;
    public readonly Func<IQueryable<TType>, IQueryable<TProjection>>? FinalProjection;

    public readonly int Offset;
    public readonly int Count;

    public QueryRequest(Func<IQueryable<TType>, IQueryable<TType>>? filter, Func<IQueryable<TType>, IQueryable<TType>>? includes, Func<IQueryable<TType>, IQueryable<TType>>? order, Func<IQueryable<TType>, IQueryable<TProjection>> finalProjection, int offset, int count) {
        ArgumentNullException.ThrowIfNull(order);
        
        Filter = filter;
        Includes = includes;
        Order = order;
        FinalProjection = finalProjection;
        
        Offset = offset;
        Count = count;
    }
}