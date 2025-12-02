namespace Conflux.Services;

public readonly struct Pageable<T> {
    public readonly int TotalItemCount;
    public readonly int PageCount;
    public readonly int Offset;

    public readonly IReadOnlyCollection<T> Items;

    public int PageSize => Items.Count;

    public Pageable(int totalItemCount, int offset, IReadOnlyCollection<T> items) {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count > totalItemCount) {
            throw new ArgumentException("Items array has more element than total item count.");
        }

        if (offset + items.Count > totalItemCount) {
            throw new ArgumentException("Offset and items array surpassed total item count.");
        }

        if (items.Count == 0) {
            if (totalItemCount != 0) {
                throw new ArgumentException("Items array must not be empty when total item count is not zero.");
            }

            TotalItemCount = PageCount = Offset = 0;
            PageCount = 0;
            Items = [];
        } else {
            TotalItemCount = totalItemCount;
            PageCount = (items.Count - 1) / items.Count + 1;
            Items = items;
            Offset = int.Min(totalItemCount, offset);
        }
    }
}