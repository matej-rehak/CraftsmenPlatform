using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Exceptions;

namespace CraftsmenPlatform.Domain.Entities;

/// <summary>
/// Child entity v CraftsmanProfile agregátu - reprezentuje skill řemeslníka
/// </summary>
public class CraftsmanSkill : BaseEntity
{
    public Guid CraftsmanProfileId { get; private set; }
    public Guid SkillId { get; private set; }

    public int? YearsOfExperience { get; private set; }

    // Private constructor pro EF Core
    private CraftsmanSkill() { }

    internal CraftsmanSkill(
        Guid craftsmanProfileId,
        Guid skillId,
        int? yearsOfExperience = null)
    {
        Id = Guid.NewGuid();
        CraftsmanProfileId = craftsmanProfileId;
        SkillId = skillId;
        YearsOfExperience = yearsOfExperience;
        CreatedAt = DateTime.UtcNow;

        if (yearsOfExperience.HasValue && yearsOfExperience.Value < 0)
            throw new BusinessRuleValidationException("Years of experience cannot be negative", "YearsOfExperienceValidation");
    }

    /// <summary>
    /// Aktualizace skill
    /// </summary>
    public Result Update(int? yearsOfExperience = null)
    {
        if (yearsOfExperience.HasValue)
        {
            if (yearsOfExperience.Value < 0)
                return Result.Failure("Years of experience cannot be negative");

            YearsOfExperience = yearsOfExperience.Value;
        }

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}