using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Common.Interfaces;
using CraftsmenPlatform.Domain.Events;
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

    // Private constructor - jen assignment, žádná validace
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
        Rating = rating;
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
    public static Result<Review> Create(
        Guid userId,
        Guid projectId,
        Guid craftsmanId,
        int ratingValue,
        string? comment = null)
    {
        // Validace Guid parametrů
        if (userId == Guid.Empty)
            return Result<Review>.Failure("User ID cannot be empty");

        if (projectId == Guid.Empty)
            return Result<Review>.Failure("Project ID cannot be empty");

        if (craftsmanId == Guid.Empty)
            return Result<Review>.Failure("Craftsman ID cannot be empty");

        // Vytvoření Rating value objectu
        var ratingResult = Rating.Create(ratingValue);
        if (ratingResult.IsFailure)
            return Result<Review>.Failure(ratingResult.Error);

        // Validace komentáře (volitelné)
        if (!string.IsNullOrWhiteSpace(comment) && comment.Length > 2000)
            return Result<Review>.Failure("Comment cannot exceed 2000 characters");

        var review = new Review(
            userId,
            projectId,
            craftsmanId,
            ratingResult.Value,
            comment);

        return Result<Review>.Success(review);
    }

    /// <summary>
    /// Publikování review
    /// </summary>
    public Result Publish()
    {
        if (IsPublished)
            return Result.Failure("Review is already published");

        IsPublished = true;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReviewPublishedEvent(Id, CraftsmanId, ProjectId, Rating.Value));

        return Result.Success();
    }

    /// <summary>
    /// Verifikace review
    /// </summary>
    public Result Verify()
    {
        if (!IsPublished)
            return Result.Failure("Cannot verify unpublished review");

        if (IsVerified)
            return Result.Failure("Review is already verified");

        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Aktualizace review (pouze před publikací)
    /// </summary>
    public Result Update(int? newRating = null, string? newComment = null)
    {
        if (IsPublished)
            return Result.Failure("Cannot update published review");

        // Aktualizace ratingu
        if (newRating.HasValue)
        {
            var ratingResult = Rating.Create(newRating.Value);
            if (ratingResult.IsFailure)
                return Result.Failure(ratingResult.Error);

            Rating = ratingResult.Value;
        }

        // Aktualizace komentáře
        if (newComment != null)
        {
            var trimmedComment = newComment.Trim();
            
            if (trimmedComment.Length > 2000)
                return Result.Failure("Comment cannot exceed 2000 characters");

            Comment = string.IsNullOrWhiteSpace(trimmedComment) ? null : trimmedComment;
        }

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Zrušení publikace (pokud je potřeba)
    /// </summary>
    public Result Unpublish()
    {
        if (!IsPublished)
            return Result.Failure("Review is not published");

        if (IsVerified)
            return Result.Failure("Cannot unpublish verified review");

        IsPublished = false;
        PublishedAt = null;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}