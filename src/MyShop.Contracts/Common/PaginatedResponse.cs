namespace MyShop.Contracts.Common;

public sealed class PaginatedResponse<T>
{
    public IReadOnlyList<T> Items { get; }
    public int PageIndex { get; }  // 1-based
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    private PaginatedResponse()
    {
        
    }
    public PaginatedResponse(
        IReadOnlyList<T> items,
        int totalCount,
        int pageIndex,
        int pageSize)
    {
        if (items is null) throw new ArgumentNullException(nameof(items));
        if (totalCount < 0) throw new ArgumentOutOfRangeException(nameof(totalCount));
        if (pageIndex < 1) throw new ArgumentOutOfRangeException(nameof(pageIndex));
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

        Items = items;
        TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public static PaginatedResponse<T> Create(
        IReadOnlyList<T> items,
        int totalCount,
        int pageIndex,
        int pageSize)
        => new(items, totalCount, pageIndex, pageSize);
}
