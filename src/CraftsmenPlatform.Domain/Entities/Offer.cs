public class Offer : SoftDeletableEntity
{
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public Guid CraftsmanId { get; private set; }
    public Craftsman Craftsman { get; private set; } = null!;

    public decimal? Price { get; private set; } = null;
    public string Description { get; private set; } = string.Empty;
    public int? EstimatedDurationDays { get; private set; } = null;
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    public OfferStatus Status { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public DateTime? RejectedAt { get; private set; }
}