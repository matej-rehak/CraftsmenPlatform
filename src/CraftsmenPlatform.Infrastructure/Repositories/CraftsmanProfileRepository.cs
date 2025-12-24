using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.Repositories;
using CraftsmenPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CraftsmenPlatform.Infrastructure.Repositories;

public class CraftsmanProfileRepository : Repository<CraftsmanProfile>, ICraftsmanProfileRepository
{
    public CraftsmanProfileRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<CraftsmanProfile?> GetByUserIdAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(cp => cp.UserId == userId, cancellationToken);
    }

    public async Task<CraftsmanProfile?> GetByIdWithSkillsAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(cp => cp.Skills)
            .FirstOrDefaultAsync(cp => cp.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<CraftsmanProfile>> GetVerifiedCraftsmenAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cp => cp.IsVerified)
            .OrderByDescending(cp => cp.AverageRating)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CraftsmanProfile>> GetAvailableBySkillAsync(
        Guid skillId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(cp => cp.Skills)
            .Where(cp => cp.IsAvailable && cp.Skills.Any(s => s.SkillId == skillId))
            .OrderByDescending(cp => cp.AverageRating)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CraftsmanProfile>> SearchAsync(
        string? searchTerm = null,
        bool? isVerified = null,
        bool? isAvailable = null,
        decimal? minRating = null,
        IEnumerable<Guid>? skillIds = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Include(cp => cp.Skills).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(cp => cp.Bio.ToLower().Contains(term));
        }

        if (isVerified.HasValue)
        {
            query = query.Where(cp => cp.IsVerified == isVerified.Value);
        }

        if (isAvailable.HasValue)
        {
            query = query.Where(cp => cp.IsAvailable == isAvailable.Value);
        }

        if (minRating.HasValue)
        {
            query = query.Where(cp => cp.AverageRating >= minRating.Value);
        }

        if (skillIds != null && skillIds.Any())
        {
            query = query.Where(cp => cp.Skills.Any(s => skillIds.Contains(s.SkillId)));
        }

        return await query
            .OrderByDescending(cp => cp.AverageRating)
            .ThenByDescending(cp => cp.CompletedProjectsCount)
            .ToListAsync(cancellationToken);
    }
}