namespace CraftsmenPlatform.Application.DTOs.Project;

/// <summary>
/// Generic paginated response wrapper
/// </summary>
public record PagedResponseDto<T>
{
    public IReadOnlyList<T> Data { get; init; } = Array.Empty<T>();
    public PaginationMetadataDto Pagination { get; init; } = new();
}