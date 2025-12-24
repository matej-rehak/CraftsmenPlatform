using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.Repositories;
using CraftsmenPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CraftsmenPlatform.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }

    public async Task<Category?> GetByIdWithSkillsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.CategorySkills)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(c => c.Name == name, cancellationToken);
    }
}