namespace CraftsmenPlatform.Application.DTOs.Project;

/// <summary>
/// Pagination metadata for API responses
/// </summary>
public record PaginationMetadataDto
{
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}