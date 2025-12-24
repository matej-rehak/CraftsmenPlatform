using CraftsmenPlatform.Domain.Entities;

namespace CraftsmenPlatform.Domain.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// Získá kategorii podle jména
    /// </summary>
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Získá kategorie včetně dovedností
    /// </summary>
    Task<Category?> GetByIdWithSkillsAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Získá aktivní kategorie
    /// </summary>
    Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Zkontroluje zda jméno kategorie existuje
    /// </summary>
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
}