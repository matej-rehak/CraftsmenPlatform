using CraftsmenPlatform.Domain.Entities;

namespace CraftsmenPlatform.Domain.Repositories;

public interface ICraftsmanProfileRepository : IRepository<CraftsmanProfile>
{
    /// <summary>
    /// Získá profil podle User ID
    /// </summary>
    Task<CraftsmanProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Získá profil včetně dovedností
    /// </summary>
    Task<CraftsmanProfile?> GetByIdWithSkillsAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Získá verifikované řemeslníky
    /// </summary>
    Task<IReadOnlyList<CraftsmanProfile>> GetVerifiedCraftsmenAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Získá dostupné řemeslníky podle dovednosti
    /// </summary>
    Task<IReadOnlyList<CraftsmanProfile>> GetAvailableBySkillAsync(
        Guid skillId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Vyhledávání řemeslníků
    /// </summary>
    Task<IReadOnlyList<CraftsmanProfile>> SearchAsync(
        string? searchTerm = null,
        bool? isVerified = null,
        bool? isAvailable = null,
        decimal? minRating = null,
        IEnumerable<Guid>? skillIds = null,
        CancellationToken cancellationToken = default);
}
