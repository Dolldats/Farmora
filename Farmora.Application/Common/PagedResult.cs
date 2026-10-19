namespace Farmora.Application.Common;

public sealed class PaginationParams
{
    private int _page = 1;
    private int _pageSize = 20;

    public int Page
    {
        get => _page;
        init => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value < 1 ? 1 : value > 100 ? 100 : value;
    }
}

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public bool HasNext { get; init; }

    public static PagedResult<T> Create(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        var totalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResult<T>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNext = page < totalPages,
            Items = items,
        };
    }
}
