using CraftsmenPlatform.Domain.Entities;

namespace CraftsmenPlatform.Domain.Repositories;

public interface ISkillRepository : IRepository<Skill>
{
    /// <summary>
    /// Získá dovednost podle jména
    /// </summary>
    Task<Skill?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Zkontroluje zda jméno dovednosti existuje
    /// </summary>
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Získá aktivní dovednosti
    /// </summary>
    Task<IReadOnlyList<Skill>> GetActiveSkillsAsync(CancellationToken cancellationToken = default);
}