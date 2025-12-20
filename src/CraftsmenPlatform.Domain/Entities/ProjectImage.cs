public class ProjectImage : BaseEntity
{
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public string ImageUrl { get; private set; } = string.Empty;
}