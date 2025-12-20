public class Project : SoftDeletableEntity
{
    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    // Budget & Timeline
    public decimal? BudgetMin { get; private set; }
    public decimal? BudgetMax { get; private set; }
    public DateTime? PreferredStartDate { get; private set; }
    public DateTime? Deadline { get; private set; }

    // Status
    public ProjectStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // Vybraná nabídka
    public Guid? AcceptedOfferId { get; private set; }
    public Offer? AcceptedOffer { get; private set; }

    // Collections
    private readonly List<Offer> _offers = new();
    public IReadOnlyCollection<Offer> Offers => _offers.AsReadOnly();
    
    private readonly List<ProjectImage> _images = new();
    public IReadOnlyCollection<ProjectImage> Images => _images.AsReadOnly();
}