using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Common.Interfaces;
using System.Linq.Expressions;

namespace CraftsmenPlatform.Domain.Repositories;

/// <summary>
/// Repository interface pro entity s soft delete
/// </summary>
public interface ISoftDeletableRepository<T> : IRepository<T> 
    where T : SoftDeletableEntity, IAggregateRoot
{
    /// <summary>
    /// Získá entity včetně smazaných
    /// </summary>
    Task<IReadOnlyList<T>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Najde entity včetně smazaných
    /// </summary>
    Task<IReadOnlyList<T>> FindIncludingDeletedAsync(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obnoví smazanou entitu
    /// </summary>
    Task<T?> RestoreAsync(Guid id, CancellationToken cancellationToken = default);
}