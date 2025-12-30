using CraftsmenPlatform.Domain.ValueObjects;
using CraftsmenPlatform.Domain.Enums;
using CraftsmenPlatform.Domain.Entities;

namespace CraftsmenPlatform.Application.DTOs.Project;

/// <summary>
/// Project DTO for API responses
/// </summary>

public record ProjectResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Money? BudgetMin { get; init; }
    public Money? BudgetMax { get; init; }
    public DateTime? PreferredStartDate { get; init; }
    public DateTime? Deadline { get; init; }
    public ProjectStatus Status { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public IReadOnlyCollection<ProjectImage> Images { get; init; }
    public bool HasAcceptedOffer { get; init; }
}

