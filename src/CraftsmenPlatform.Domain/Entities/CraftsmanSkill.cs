public class CraftsmanSkill : BaseEntity
{
    public Guid CraftsmanId { get; private set; }
    public Craftsman Craftsman { get; private set; } = null!;

    public Guid SkillId { get; private set; }
    public Skill Skill { get; private set; } = null!;
}