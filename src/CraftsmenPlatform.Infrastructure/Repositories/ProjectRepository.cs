using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.Repositories;
using CraftsmenPlatform.Infrastructure.Persistence;
using CraftsmenPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CraftsmenPlatform.Infrastructure.Repositories;

public class ProjectRepository : SoftDeletableRepository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Project>> GetByCustomerIdAsync(
        Guid customerId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetByStatusAsync(
        ProjectStatus status, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetPublishedProjectsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Status == ProjectStatus.Published)
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Project?> GetByIdWithOffersAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Offers)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Project?> GetByIdWithImagesAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> SearchAsync(
        string? searchTerm = null,
        ProjectStatus? status = null,
        decimal? minBudget = null,
        decimal? maxBudget = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p => 
                p.Title.ToLower().Contains(term) || 
                p.Description.ToLower().Contains(term));
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (minBudget.HasValue)
        {
            query = query.Where(p => p.BudgetMin.Amount >= minBudget.Value);
        }

        if (maxBudget.HasValue)
        {
            query = query.Where(p => p.BudgetMax.Amount <= maxBudget.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(p => p.PreferredStartDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(p => p.PreferredStartDate <= endDate.Value);
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}