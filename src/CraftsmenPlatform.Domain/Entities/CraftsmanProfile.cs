using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Common.Interface;
using CraftsmenPlatform.Domain.Exceptions;
using CraftsmenPlatform.Domain.ValueObjects;

namespace CraftsmenPlatform.Domain.Entities;

/// <summary>
/// Agregát CraftsmanProfile - reprezentuje profil řemeslníka
/// </summary>
public class CraftsmanProfile : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }

    public string? Bio { get; private set; }
    public string? RegistrationNumber { get; private set; } // IČO
    public string? TaxNumber { get; private set; } // DIČ
    public int? YearsOfExperience { get; private set; }

    // Rating
    public decimal AverageRating { get; private set; }
    public int TotalReviews { get; private set; }
    public int CompletedProjectsCount { get; private set; }

    // Status
    public bool IsVerified { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public bool IsAvailable { get; private set; }

    // Skills - child entities
    private readonly List<CraftsmanSkill> _skills = new();
    public IReadOnlyCollection<CraftsmanSkill> Skills => _skills.AsReadOnly();

    // Private constructor pro EF Core
    private CraftsmanProfile() { }

    private CraftsmanProfile(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        AverageRating = 0;
        TotalReviews = 0;
        CompletedProjectsCount = 0;
        IsVerified = false;
        IsAvailable = true;
    }

    /// <summary>
    /// Factory metoda pro vytvoření nového craftsman profilu
    /// </summary>
    public static CraftsmanProfile Create(Guid userId)
    {
        return new CraftsmanProfile(userId);
    }

    /// <summary>
    /// Aktualizace profilu
    /// </summary>
    public void UpdateProfile(
        string? bio = null,
        string? registrationNumber = null,
        string? taxNumber = null,
        int? yearsOfExperience = null)
    {
        if (!string.IsNullOrWhiteSpace(bio))
            Bio = bio.Trim();

        if (!string.IsNullOrWhiteSpace(registrationNumber))
            RegistrationNumber = registrationNumber.Trim();

        if (!string.IsNullOrWhiteSpace(taxNumber))
            TaxNumber = taxNumber.Trim();

        if (yearsOfExperience.HasValue)
        {
            if (yearsOfExperience.Value < 0)
                throw new BusinessRuleValidationException(
                    nameof(YearsOfExperience),
                    "Years of experience cannot be negative");

            YearsOfExperience = yearsOfExperience.Value;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifikace profilu
    /// </summary>
    public Result Verify()
    {
        if (IsVerified)
            return Result.Failure("Profile is already verified");

        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Zrušení verifikace
    /// </summary>
    public Result Unverify()
    {
        if (!IsVerified)
            return Result.Failure("Profile is not verified");

        IsVerified = false;
        VerifiedAt = null;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Nastavení dostupnosti
    /// </summary>
    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Přidání skill
    /// </summary>
    public Result AddSkill(Guid skillId, int? yearsOfExperience = null, string? certificationLevel = null)
    {
        if (_skills.Any(s => s.SkillId == skillId))
            return Result.Failure("Skill already added to profile");

        var craftsmanSkill = new CraftsmanSkill(Id, skillId, yearsOfExperience, certificationLevel);
        _skills.Add(craftsmanSkill);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Odebrání skill
    /// </summary>
    public Result RemoveSkill(Guid skillId)
    {
        var skill = _skills.FirstOrDefault(s => s.SkillId == skillId);
        if (skill == null)
            return Result.Failure("Skill not found in profile");

        _skills.Remove(skill);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Aktualizace ratingu (internal - volá se když je publikována review)
    /// </summary>
    internal void UpdateRating(Rating newRating)
    {
        // Přepočet průměrného ratingu
        var totalPoints = (AverageRating * TotalReviews) + newRating.Value;
        TotalReviews++;
        AverageRating = totalPoints / TotalReviews;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Inkrementování dokončených projektů
    /// </summary>
    internal void IncrementCompletedProjects()
    {
        CompletedProjectsCount++;
        UpdatedAt = DateTime.UtcNow;
    }
}