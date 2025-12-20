public class CraftsmanProfile : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public string? Bio { get; private set; }
    public string? RegistrationNumber { get; private set; } // IČO
    public string? TaxNumber { get; private set; } // DIČ
    public int? YearsOfExperience { get; private set; }

    // Rating
    public decimal AverageRating { get; private set; }
    public int TotalReviews { get; private set; }
    public int CompletedProjectsCount { get; private set; }

    // Status
    public bool IsVerified { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public bool IsAvailable { get; private set; }



}