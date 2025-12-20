public class CustomerProfile : BaseEntity
{
    // Vzta k user (1:1)
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    // Statistiky
    public int TotalProjectsCreated { get; private set; }
    public int CompletedProjects { get; private set; }

}