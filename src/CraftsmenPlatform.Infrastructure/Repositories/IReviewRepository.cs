using CraftsmenPlatform.Domain.Entities;

namespace CraftsmenPlatform.Domain.Repositories;

public interface IReviewRepository : ISoftDeletableRepository<Review>
{
    /// <summary>
    /// Získá recenze řemeslníka
    /// </summary>
    Task<IReadOnlyList<Review>> GetByCraftsmanIdAsync(Guid craftsmanId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Získá recenze projektu
    /// </summary>
    Task<Review?> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Získá publikované recenze řemeslníka
    /// </summary>
    Task<IReadOnlyList<Review>> GetPublishedByCraftsmanIdAsync(
        Guid craftsmanId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Zkontroluje zda uživatel již recenzoval projekt
    /// </summary>
    Task<bool> HasUserReviewedProjectAsync(
        Guid userId, 
        Guid projectId, 
        CancellationToken cancellationToken = default);
}