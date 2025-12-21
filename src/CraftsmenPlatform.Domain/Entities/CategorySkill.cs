using CraftsmenPlatform.Domain.Common;

namespace CraftsmenPlatform.Domain.Entities;

/// <summary>
/// Child entity - reprezentuje many-to-many vztah mezi Category a Skill
/// </summary>
public class CategorySkill : BaseEntity
{
    public Guid CategoryId { get; private set; }
    public Guid SkillId { get; private set; }

    // Private constructor pro EF Core
    private CategorySkill() { }

    internal CategorySkill(Guid categoryId, Guid skillId)
    {
        Id = Guid.NewGuid();
        CategoryId = categoryId;
        SkillId = skillId;
        CreatedAt = DateTime.UtcNow;
    }
}
