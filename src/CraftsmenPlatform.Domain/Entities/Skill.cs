public class Skill : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; } = string.Empty;
    public string? IconUrl { get; private set; } = string.Empty;
}