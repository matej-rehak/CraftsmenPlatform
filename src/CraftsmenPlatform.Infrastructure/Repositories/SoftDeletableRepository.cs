using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Repositories;
using CraftsmenPlatform.Domain.Common.Interfaces;
using CraftsmenPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CraftsmenPlatform.Infrastructure.Repositories;

public class SoftDeletableRepository<T> : Repository<T>, ISoftDeletableRepository<T> 
    where T : SoftDeletableEntity, IAggregateRoot
{
    public SoftDeletableRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<T>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.IgnoreQueryFilters().ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<T>> FindIncludingDeletedAsync(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.IgnoreQueryFilters().Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<T?> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity != null)
        {
            entity.Restore();
        }

        return entity;
    }
}
