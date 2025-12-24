using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Common.Interface;
using CraftsmenPlatform.Domain.Exceptions;

namespace CraftsmenPlatform.Domain.Entities;

/// <summary>
/// Agregát Category - reprezentuje kategorii dovedností
/// </summary>
public class Category : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? IconUrl { get; private set; }

    // Many-to-many relationship s Skills přes CategorySkill
    private readonly List<CategorySkill> _categorySkills = new();
    public IReadOnlyCollection<CategorySkill> CategorySkills => _categorySkills.AsReadOnly();

    // Private constructor pro EF Core
    private Category() { }

    private Category(string name, string? description = null, string? iconUrl = null)
    {
        Id = Guid.NewGuid();
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
        Description = description?.Trim();
        IconUrl = iconUrl?.Trim();
        CreatedAt = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleValidationException(nameof(Name), "Category name cannot be empty");

        if (name.Length > 100)
            throw new BusinessRuleValidationException(nameof(Name), "Category name cannot exceed 100 characters");
    }

    /// <summary>
    /// Factory metoda pro vytvoření nové kategorie
    /// </summary>
    public static Category Create(string name, string? description = null, string? iconUrl = null)
    {
        return new Category(name, description, iconUrl);
    }

    /// <summary>
    /// Aktualizace kategorie
    /// </summary>
    public Result Update(string? name = null, string? description = null, string? iconUrl = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            if (name.Length > 100)
                return Result.Failure("Category name cannot exceed 100 characters");

            Name = name.Trim();
        }

        if (description != null)
            Description = description.Trim();

        if (iconUrl != null)
            IconUrl = iconUrl.Trim();

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Přidání skill do kategorie
    /// </summary>
    public Result AddSkill(Guid skillId)
    {
        if (_categorySkills.Any(cs => cs.SkillId == skillId))
            return Result.Failure("Skill is already in this category");

        var categorySkill = new CategorySkill(Id, skillId);
        _categorySkills.Add(categorySkill);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Odebrání skill z kategorie
    /// </summary>
    public Result RemoveSkill(Guid skillId)
    {
        var categorySkill = _categorySkills.FirstOrDefault(cs => cs.SkillId == skillId);
        if (categorySkill == null)
            return Result.Failure("Skill not found in this category");

        _categorySkills.Remove(categorySkill);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
