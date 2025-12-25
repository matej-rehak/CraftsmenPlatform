using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.ValueObjects;
using CraftsmenPlatform.Domain.Enums;

namespace CraftsmenPlatform.Domain.Repositories;

public interface IProjectRepository : ISoftDeletableRepository<Project>
{
    /// <summary>
    /// Získá projekty zákazníka
    /// </summary>
    Task<IReadOnlyList<Project>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Získá projekty podle statusu
    /// </summary>
    Task<IReadOnlyList<Project>> GetByStatusAsync(ProjectStatus status, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Získá publikované projekty (pro řemeslníky)
    /// </summary>
    Task<IReadOnlyList<Project>> GetPublishedProjectsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Získá projekt včetně nabídek
    /// </summary>
    Task<Project?> GetByIdWithOffersAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Získá projekt včetně obrázků
    /// </summary>
    Task<Project?> GetByIdWithImagesAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Vyhledávání projektů podle kritérií
    /// </summary>
    Task<IReadOnlyList<Project>> SearchAsync(
        string? searchTerm = null,
        ProjectStatus? status = null,
        decimal? minBudget = null,
        decimal? maxBudget = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
}