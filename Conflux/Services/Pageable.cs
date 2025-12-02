namespace Conflux.Services;

public readonly struct Pageable<T> {
    public readonly int TotalItemCount;
    public readonly int PageCount;
    public readonly int Offset;

    public readonly IReadOnlyCollection<T> Items;

    public int PageSize => Items.Count;

    public Pageable(int totalItemCount, int offset, IReadOnlyCollection<T> items) {
        ArgumentNullException.ThrowIfNull(items);
        
        TotalItemCount = totalItemCount;
        PageCount = (items.Count - 1) / items.Count + 1;
        Items = items;
        Offset = int.Min(totalItemCount, offset);
    }
}