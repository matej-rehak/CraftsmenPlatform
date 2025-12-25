using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.ValueObjects;
using CraftsmenPlatform.Domain.Enums;

namespace CraftsmenPlatform.Domain.Repositories;

public interface IUserRepository : ISoftDeletableRepository<User>
{
    /// <summary>
    /// Najde uživatele podle emailu
    /// </summary>
    Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Zkontroluje zda email již existuje
    /// </summary>
    Task<bool> EmailExistsAsync(EmailAddress email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Získá uživatele podle role
    /// </summary>
    Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Najít uživatele podle refresh tokenu
    /// </summary>
    Task<User?> GetByRefreshTokenAsync(
        string refreshToken, 
        CancellationToken cancellationToken = default);
}