using CraftsmenPlatform.Domain.Entities;

namespace CraftsmenPlatform.Domain.Repositories;

public interface ICustomerProfileRepository : IRepository<CustomerProfile>
{
    /// <summary>
    /// Získá profil podle User ID
    /// </summary>
    Task<CustomerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}