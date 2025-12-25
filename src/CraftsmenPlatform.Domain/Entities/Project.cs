using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Common.Interfaces;
using CraftsmenPlatform.Domain.Enums;
using CraftsmenPlatform.Domain.Events;
using CraftsmenPlatform.Domain.Exceptions;
using CraftsmenPlatform.Domain.ValueObjects;

namespace CraftsmenPlatform.Domain.Entities;

/// <summary>
/// Agregát Project - reprezentuje projekt zadaný zákazníkem
/// </summary>
public class Project : SoftDeletableEntity, IAggregateRoot
{
    public Guid CustomerId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    // Budget
    public Money? BudgetMin { get; private set; }
    public Money? BudgetMax { get; private set; }

    // Timeline
    public DateTime? PreferredStartDate { get; private set; }
    public DateTime? Deadline { get; private set; }

    // Status
    public ProjectStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // Vybraná nabídka
    public Guid? AcceptedOfferId { get; private set; }

    // Collections - child entities
    private readonly List<Offer> _offers = new();
    public IReadOnlyCollection<Offer> Offers => _offers.AsReadOnly();

    private readonly List<ProjectImage> _images = new();
    public IReadOnlyCollection<ProjectImage> Images => _images.AsReadOnly();

    public Offer? AcceptedOffer => _offers.FirstOrDefault(o => o.Id == AcceptedOfferId);
    public bool HasAcceptedOffer => AcceptedOfferId.HasValue;

    // Private constructor pro EF Core
    private Project() { }

    private Project(
        Guid customerId,
        string title,
        string description,
        Money? budgetMin = null,
        Money? budgetMax = null,
        DateTime? preferredStartDate = null,
        DateTime? deadline = null)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        Title = title?.Trim() ?? throw new ArgumentNullException(nameof(title));
        Description = description?.Trim() ?? throw new ArgumentNullException(nameof(description));
        BudgetMin = budgetMin;
        BudgetMax = budgetMax;
        PreferredStartDate = preferredStartDate;
        Deadline = deadline;
        Status = ProjectStatus.Draft;
        CreatedAt = DateTime.UtcNow;

        // Constructor validace - používají exceptions (technical validation)
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleValidationException(nameof(Title), "Project title cannot be empty");

        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessRuleValidationException(nameof(Description), "Project description cannot be empty");

        if (budgetMin != null && budgetMax != null && budgetMin.IsGreaterThan(budgetMax))
            throw new BusinessRuleValidationException(nameof(BudgetMin), "Minimum budget cannot be greater than maximum budget");

        if (preferredStartDate.HasValue && deadline.HasValue && preferredStartDate.Value > deadline.Value)
            throw new BusinessRuleValidationException(nameof(PreferredStartDate), "Preferred start date cannot be after deadline");
    }

    /// <summary>
    /// Factory metoda pro vytvoření nového projektu
    /// </summary>
    public static Project Create(
        Guid customerId,
        string title,
        string description,
        decimal? budgetMinAmount = null,
        decimal? budgetMaxAmount = null,
        string currency = "CZK",
        DateTime? preferredStartDate = null,
        DateTime? deadline = null)
    {
        Money? budgetMin = budgetMinAmount.HasValue ? Money.Create(budgetMinAmount.Value, currency) : null;
        Money? budgetMax = budgetMaxAmount.HasValue ? Money.Create(budgetMaxAmount.Value, currency) : null;

        return new Project(
            customerId,
            title,
            description,
            budgetMin,
            budgetMax,
            preferredStartDate,
            deadline);
    }

    /// <summary>
    /// Publikování projektu
    /// </summary>
    public Result Publish()
    {
        if (Status != ProjectStatus.Draft)
            return Result.Failure($"Cannot publish project with status {Status}");

        Status = ProjectStatus.Published;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectPublishedEvent(Id, CustomerId, Title));

        return Result.Success();
    }

    /// <summary>
    /// Přidání nabídky od řemeslníka
    /// </summary>
    public Result<Offer> AddOffer(
        Guid craftsmanId,
        decimal priceAmount,
        string description,
        int? estimatedDurationDays = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string currency = "CZK")
    {
        if (Status != ProjectStatus.Published)
            return Result<Offer>.Failure("Cannot add offer to non-published project");

        var price = Money.Create(priceAmount, currency);
        var offer = new Offer(Id, craftsmanId, price, description, estimatedDurationDays, startDate, endDate);

        _offers.Add(offer);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OfferSubmittedEvent(offer.Id, Id, craftsmanId, priceAmount));

        return Result<Offer>.Success(offer);
    }

    /// <summary>
    /// Akceptování nabídky
    /// </summary>
    public Result AcceptOffer(Guid offerId)
    {
        if (Status != ProjectStatus.Published)
            return Result.Failure($"Cannot accept offer for project with status {Status}");

        if (HasAcceptedOffer)
            return Result.Failure("Project already has an accepted offer");

        var offer = _offers.FirstOrDefault(o => o.Id == offerId);
        if (offer == null)
            return Result.Failure("Offer not found");

        offer.Accept();
        AcceptedOfferId = offerId;
        Status = ProjectStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;

        // Reject all other pending offers
        foreach (var otherOffer in _offers.Where(o => o.Id != offerId && o.Status == OfferStatus.Pending))
        {
            otherOffer.Reject();
        }

        AddDomainEvent(new OfferAcceptedEvent(offerId, Id, offer.CraftsmanId));

        return Result.Success();
    }

    /// <summary>
    /// Dokončení projektu
    /// </summary>
    public Result Complete()
    {
        if (Status != ProjectStatus.InProgress)
            return Result.Failure($"Cannot complete project with status {Status}");

        if (!HasAcceptedOffer)
            return Result.Failure("Cannot complete project without accepted offer");

        Status = ProjectStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        var acceptedOffer = AcceptedOffer!;
        AddDomainEvent(new ProjectCompletedEvent(Id, CustomerId, acceptedOffer.CraftsmanId));

        return Result.Success();
    }

    /// <summary>
    /// Zrušení projektu
    /// </summary>
    public Result Cancel(string reason)
    {
        if (Status == ProjectStatus.Completed || Status == ProjectStatus.Cancelled)
            return Result.Failure($"Cannot cancel project with status {Status}");

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure("Cancellation reason cannot be empty");

        Status = ProjectStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        // Reject all pending offers
        foreach (var offer in _offers.Where(o => o.Status == OfferStatus.Pending))
        {
            offer.Reject();
        }

        return Result.Success();
    }

    /// <summary>
    /// Přidání obrázku k projektu
    /// </summary>
    public Result AddImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return Result.Failure("Image URL cannot be empty");

        var displayOrder = _images.Count;
        var image = new ProjectImage(Id, imageUrl, displayOrder);
        _images.Add(image);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Odebrání obrázku
    /// </summary>
    public Result RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
            return Result.Failure("Image not found");

        _images.Remove(image);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Aktualizace projektu (pouze v Draft stavu)
    /// </summary>
    public Result Update(
        string? title = null,
        string? description = null,
        Money? budgetMin = null,
        Money? budgetMax = null,
        DateTime? preferredStartDate = null,
        DateTime? deadline = null)
    {
        if (Status != ProjectStatus.Draft)
            return Result.Failure("Can only update draft projects");

        if (!string.IsNullOrWhiteSpace(title))
            Title = title.Trim();

        if (!string.IsNullOrWhiteSpace(description))
            Description = description.Trim();

        if (budgetMin != null)
            BudgetMin = budgetMin;

        if (budgetMax != null)
            BudgetMax = budgetMax;

        if (preferredStartDate.HasValue)
            PreferredStartDate = preferredStartDate;

        if (deadline.HasValue)
            Deadline = deadline;

        if (BudgetMin != null && BudgetMax != null && BudgetMin.IsGreaterThan(BudgetMax))
            return Result.Failure("Minimum budget cannot be greater than maximum budget");

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}