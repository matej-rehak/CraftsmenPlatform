using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Common.Interfaces;
using CraftsmenPlatform.Domain.Exceptions;

namespace CraftsmenPlatform.Domain.Entities;

/// <summary>
/// Agregát Skill - reprezentuje skill/dovednost
/// </summary>
public class Skill : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? IconUrl { get; private set; }

    // Many-to-many relationship s Categories přes CategorySkill
    private readonly List<CategorySkill> _categorySkills = new();
    public IReadOnlyCollection<CategorySkill> CategorySkills => _categorySkills.AsReadOnly();

    // Private constructor pro EF Core
    private Skill() { }

    private Skill(string name, string? description = null, string? iconUrl = null)
    {
        Id = Guid.NewGuid();
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
        Description = description?.Trim();
        IconUrl = iconUrl?.Trim();
        CreatedAt = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleValidationException(nameof(Name), "Skill name cannot be empty");

        if (name.Length > 100)
            throw new BusinessRuleValidationException(nameof(Name), "Skill name cannot exceed 100 characters");
    }

    /// <summary>
    /// Factory metoda pro vytvoření nového skill
    /// </summary>
    public static Skill Create(string name, string? description = null, string? iconUrl = null)
    {
        return new Skill(name, description, iconUrl);
    }

    /// <summary>
    /// Aktualizace skill
    /// </summary>
    public Result Update(string? name = null, string? description = null, string? iconUrl = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            if (name.Length > 100)
                return Result.Failure("Skill name cannot exceed 100 characters");

            Name = name.Trim();
        }

        if (description != null)
            Description = description.Trim();

        if (iconUrl != null)
            IconUrl = iconUrl.Trim();

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
