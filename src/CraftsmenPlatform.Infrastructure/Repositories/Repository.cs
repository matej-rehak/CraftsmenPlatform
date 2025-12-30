using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Common.Interfaces;
using CraftsmenPlatform.Domain.Repositories;
using CraftsmenPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CraftsmenPlatform.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity, IAggregateRoot
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            return await _dbSet.CountAsync(cancellationToken);
        
        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var entry = await _dbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public async Task<PagedResult<T>> GetPagedAsync(Specification<T> specification, CancellationToken cancellationToken = default)
    {
        // Get total count (without pagination)
        var countSpec = CreateCountSpecification(specification);
        var totalCount = await CountAsync(countSpec, cancellationToken);

        if (totalCount == 0)
        {
            return PagedResult<T>.Empty(
                specification.Skip / specification.Take + 1,
                specification.Take);
        }

        // Apply specification with pagination
        var query = SpecificationEvaluator.GetQuery(_dbSet.AsQueryable(), specification);
        var items = await query.ToListAsync(cancellationToken);

        var pageNumber = specification.Skip / specification.Take + 1;
        return new PagedResult<T>(items, pageNumber, specification.Take, totalCount);
    }

    public async Task<PagedResult<T>> GetPagedAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var totalCount = await _dbSet.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return PagedResult<T>.Empty(pagination.PageNumber, pagination.PageSize);
        }

        var items = await _dbSet
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(
            items,
            pagination.PageNumber,
            pagination.PageSize,
            totalCount);
    }

    public async Task<int> CountAsync(Specification<T> specification, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        return await query.CountAsync(cancellationToken);
    }

    private static Specification<T> CreateCountSpecification(Specification<T> spec)
    {
        // Create a new specification with same criteria but no includes/paging
        return new CountSpecification<T>(spec.Criteria);
    }
}

/// <summary>
/// Simple specification for counting only
/// </summary>
internal class CountSpecification<T> : Specification<T>
{
    public CountSpecification(Expression<Func<T, bool>>? criteria)
    {
        Criteria = criteria;
    }
}
