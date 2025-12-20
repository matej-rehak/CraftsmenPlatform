public class Review : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public Guid CraftsmanId { get; private set; }
    public Craftsman Craftsman { get; private set; } = null!;

    public int Rating { get; private set; } // 1-10
    public string? Comment { get; private set; } = string.Empty;
    public bool IsVerified { get; private set; }

    public DateTime? PublishedAt { get; private set; }
}