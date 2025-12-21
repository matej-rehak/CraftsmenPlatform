using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Enums;
using CraftsmenPlatform.Domain.Exceptions;
using CraftsmenPlatform.Domain.ValueObjects;

namespace CraftsmenPlatform.Domain.Entities;

/// <summary>
/// Child entity v Project agregátu - reprezentuje nabídku řemeslníka na projekt
/// </summary>
public class Offer : SoftDeletableEntity
{
    public Guid ProjectId { get; private set; }
    public Guid CraftsmanId { get; private set; }

    public Money Price { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public int? EstimatedDurationDays { get; private set; }
    public DateRange? Timeline { get; private set; }

    public OfferStatus Status { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public DateTime? RejectedAt { get; private set; }

    // Private constructor pro EF Core
    private Offer() { }

    internal Offer(
        Guid projectId,
        Guid craftsmanId,
        Money price,
        string description,
        int? estimatedDurationDays = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        CraftsmanId = craftsmanId;
        Price = price ?? throw new ArgumentNullException(nameof(price));
        Description = description?.Trim() ?? throw new ArgumentNullException(nameof(description));
        EstimatedDurationDays = estimatedDurationDays;
        Status = OfferStatus.Pending;
        CreatedAt = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessRuleValidationException(nameof(Description), "Offer description cannot be empty");

        if (estimatedDurationDays.HasValue && estimatedDurationDays.Value <= 0)
            throw new BusinessRuleValidationException(nameof(EstimatedDurationDays), "Duration must be positive");

        if (startDate.HasValue && endDate.HasValue)
        {
            Timeline = DateRange.Create(startDate.Value, endDate.Value);
        }
    }

    /// <summary>
    /// Akceptování nabídky (volá Project aggregate)
    /// </summary>
    internal void Accept()
    {
        if (Status != OfferStatus.Pending)
            throw new BusinessRuleValidationException(
                nameof(Accept),
                $"Cannot accept offer with status {Status}");

        Status = OfferStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Odmítnutí nabídky
    /// </summary>
    internal void Reject()
    {
        if (Status != OfferStatus.Pending)
            throw new BusinessRuleValidationException(
                nameof(Reject),
                $"Cannot reject offer with status {Status}");

        Status = OfferStatus.Rejected;
        RejectedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Aktualizace nabídky (lze jen pokud je pending)
    /// </summary>
    public Result Update(Money? newPrice = null, string? newDescription = null, int? newDuration = null)
    {
        if (Status != OfferStatus.Pending)
            return Result.Failure("Cannot update non-pending offer");

        if (newPrice != null)
            Price = newPrice;

        if (!string.IsNullOrWhiteSpace(newDescription))
            Description = newDescription.Trim();

        if (newDuration.HasValue)
        {
            if (newDuration.Value <= 0)
                return Result.Failure("Duration must be positive");
            
            EstimatedDurationDays = newDuration.Value;
        }

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
