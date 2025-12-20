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
    public string? CertificationLevel { get; private set; }

    // Private constructor pro EF Core
    private CraftsmanSkill() { }

    internal CraftsmanSkill(
        Guid craftsmanProfileId,
        Guid skillId,
        int? yearsOfExperience = null,
        string? certificationLevel = null)
    {
        Id = Guid.NewGuid();
        CraftsmanProfileId = craftsmanProfileId;
        SkillId = skillId;
        YearsOfExperience = yearsOfExperience;
        CertificationLevel = certificationLevel?.Trim();
        CreatedAt = DateTime.UtcNow;

        if (yearsOfExperience.HasValue && yearsOfExperience.Value < 0)
            throw new BusinessRuleValidationException(
                nameof(YearsOfExperience),
                "Years of experience cannot be negative");
    }

    /// <summary>
    /// Aktualizace skill
    /// </summary>
    public void Update(int? yearsOfExperience = null, string? certificationLevel = null)
    {
        if (yearsOfExperience.HasValue)
        {
            if (yearsOfExperience.Value < 0)
                throw new BusinessRuleValidationException(
                    nameof(YearsOfExperience),
                    "Years of experience cannot be negative");

            YearsOfExperience = yearsOfExperience.Value;
        }

        if (!string.IsNullOrWhiteSpace(certificationLevel))
            CertificationLevel = certificationLevel.Trim();

        UpdatedAt = DateTime.UtcNow;
    }
}