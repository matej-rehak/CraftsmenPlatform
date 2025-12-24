namespace CraftsmenPlatform.Domain.Common;

/// <summary>
/// Unit of Work pattern interface
/// Zajišťuje transakční konzistenci napříč agregáty
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Uloží všechny změny do databáze
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Začne novou transakci
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Potvrdí transakci
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Zruší transakci
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}