namespace CraftsmenPlatform.Domain.Common;

/// <summary>
/// Generic paginated result wrapper
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    /// <summary>
    /// Map items to different type (useful for DTOs)
    /// </summary>
    public PagedResult<TDestination> Map<TDestination>(
        Func<T, TDestination> mapper)
    {
        var mappedItems = Items.Select(mapper).ToList();
        return new PagedResult<TDestination>(
            mappedItems,
            PageNumber,
            PageSize,
            TotalCount);
    }

    /// <summary>
    /// Create empty result
    /// </summary>
    public static PagedResult<T> Empty(int pageNumber, int pageSize)
    {
        return new PagedResult<T>(
            Array.Empty<T>(),
            pageNumber,
            pageSize,
            0);
    }
}