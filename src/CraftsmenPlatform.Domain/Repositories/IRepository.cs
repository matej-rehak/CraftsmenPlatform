using CraftsmenPlatform.Domain.Common.Interfaces;
using CraftsmenPlatform.Domain.Common;
using System.Linq.Expressions;

namespace CraftsmenPlatform.Domain.Repositories;

/// <summary>
/// Base repository interface pro všechny aggregate roots
/// </summary>
/// <typeparam name="T">Aggregate root entity</typeparam>
public interface IRepository<T> where T : BaseEntity, IAggregateRoot
{
    // === QUERY OPERATIONS ===
    
    /// <summary>
    /// Získá entitu podle ID
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Získá všechny entity
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Najde entity podle podmínky
    /// </summary>
    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Najde první entitu podle podmínky nebo null
    /// </summary>
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Zkontroluje zda existuje entita podle podmínky
    /// </summary>
    Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Spočítá entity podle podmínky
    /// </summary>
    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null, 
        CancellationToken cancellationToken = default);
    
    // === COMMAND OPERATIONS ===
    
    /// <summary>
    /// Přidá novou entitu
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Přidá více entit najednou
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Aktualizuje entitu (tracked by EF, no explicit call needed usually)
    /// </summary>
    void Update(T entity);
    
    /// <summary>
    /// Smaže entitu
    /// </summary>
    void Remove(T entity);
    
    /// <summary>
    /// Smaže více entit najednou
    /// </summary>
    void RemoveRange(IEnumerable<T> entities);
}