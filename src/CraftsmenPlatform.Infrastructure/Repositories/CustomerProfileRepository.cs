using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.Repositories;
using CraftsmenPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CraftsmenPlatform.Infrastructure.Repositories;

public class CustomerProfileRepository : Repository<CustomerProfile>, ICustomerProfileRepository
{
    public CustomerProfileRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<CustomerProfile?> GetByUserIdAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(cp => cp.UserId == userId, cancellationToken);
    }
}