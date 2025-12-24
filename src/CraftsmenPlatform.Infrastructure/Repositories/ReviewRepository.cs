using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.Repositories;
using CraftsmenPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CraftsmenPlatform.Infrastructure.Repositories;

public class ReviewRepository : SoftDeletableRepository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Review>> GetByCraftsmanIdAsync(
        Guid craftsmanId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.CraftsmanId == craftsmanId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Review?> GetByProjectIdAsync(
        Guid projectId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.ProjectId == projectId, cancellationToken);
    }

    public async Task<IReadOnlyList<Review>> GetPublishedByCraftsmanIdAsync(
        Guid craftsmanId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.CraftsmanId == craftsmanId && r.IsPublished)
            .OrderByDescending(r => r.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasUserReviewedProjectAsync(
        Guid userId, 
        Guid projectId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(r => r.UserId == userId && r.ProjectId == projectId, cancellationToken);
    }
}