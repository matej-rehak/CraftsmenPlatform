using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Common.Interfaces;
using CraftsmenPlatform.Domain.Events;
using CraftsmenPlatform.Domain.Exceptions;
using CraftsmenPlatform.Domain.ValueObjects;

namespace CraftsmenPlatform.Domain.Entities;

/// <summary>
/// Agregát Review - reprezentuje hodnocení řemeslníka
/// </summary>
public class Review : SoftDeletableEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid CraftsmanId { get; private set; }

    public Rating Rating { get; private set; } = null!;
    public string? Comment { get; private set; }
    public bool IsVerified { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTime? PublishedAt { get; private set; }

    // Private constructor pro EF Core
    private Review() { }

    private Review(
        Guid userId,
        Guid projectId,
        Guid craftsmanId,
        Rating rating,
        string? comment = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        ProjectId = projectId;
        CraftsmanId = craftsmanId;
        Rating = rating ?? throw new ArgumentNullException(nameof(rating));
        Comment = comment?.Trim();
        IsVerified = false;
        VerifiedAt = null;
        IsPublished = false;
        PublishedAt = null;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory metoda pro vytvoření nového review
    /// </summary>
    public static Review Create(
        Guid userId,
        Guid projectId,
        Guid craftsmanId,
        int ratingValue,
        string? comment = null)
    {
        var rating = Rating.Create(ratingValue);
        return new Review(userId, projectId, craftsmanId, rating, comment);
    }

    /// <summary>
    /// Publikování review
    /// </summary>
    public Result Publish()
    {
        if (PublishedAt.HasValue)
            return Result.Failure("Review is already published");

        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsPublished = true;

        AddDomainEvent(new ReviewPublishedEvent(Id, CraftsmanId, ProjectId, Rating.Value));

        return Result.Success();
    }

    /// <summary>
    /// Verifikace review
    /// </summary>
    public Result Verify()
    {
        if (!PublishedAt.HasValue)
            return Result.Failure("Cannot verify unpublished review");

        if (IsVerified)
            return Result.Failure("Review is already verified");

        IsVerified = true;
        UpdatedAt = DateTime.UtcNow;
        VerifiedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Aktualizace review (pouze před publikací)
    /// </summary>
    public Result Update(int? newRating = null, string? newComment = null)
    {
        if (PublishedAt.HasValue)
            return Result.Failure("Cannot update published review");

        if (newRating.HasValue)
            Rating = Rating.Create(newRating.Value);

        if (newComment != null)
            Comment = newComment.Trim();

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}