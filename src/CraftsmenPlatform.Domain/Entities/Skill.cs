using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Exceptions;

namespace CraftsmenPlatform.Domain.Entities;

/// <summary>
/// Entity Skill - reprezentuje skill/dovednost (může být reference data nebo agregát)
/// </summary>
public class Skill : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? IconUrl { get; private set; }

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
    public void Update(string? name = null, string? description = null, string? iconUrl = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name.Trim();

        if (description != null)
            Description = description.Trim();

        if (iconUrl != null)
            IconUrl = iconUrl.Trim();

        UpdatedAt = DateTime.UtcNow;
    }
}