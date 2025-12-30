using CraftsmenPlatform.Domain.Enums;

namespace CraftsmenPlatform.Application.DTOs.Project;

/// <summary>
/// Request model for filtering projects
/// </summary>
public record GetProjectsRequest
{
    /// <summary>Project status filter</summary>
    public ProjectStatus? Status { get; init; }

    /// <summary>Customer ID filter</summary>
    public Guid? CustomerId { get; init; }

    /// <summary>Search term for title and description</summary>
    public string? Search { get; init; }

    /// <summary>Minimum budget</summary>
    public decimal? MinBudget { get; init; }

    /// <summary>Maximum budget</summary>
    public decimal? MaxBudget { get; init; }

    /// <summary>City filter</summary>
    public string? City { get; init; }

    /// <summary>State filter</summary>
    public string? State { get; init; }

    /// <summary>Created after date</summary>
    public DateTime? CreatedAfter { get; init; }

    /// <summary>Created before date</summary>
    public DateTime? CreatedBefore { get; init; }

    /// <summary>Page number (default: 1)</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Page size (default: 20, max: 100)</summary>
    public int PageSize { get; init; } = 20;

    /// <summary>Sort field and direction (e.g., "createdAt:desc", "title:asc")</summary>
    public string? Sort { get; init; }
}