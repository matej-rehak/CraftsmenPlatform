using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.ValueObjects;
using CraftsmenPlatform.Domain.Repositories;
using CraftsmenPlatform.Infrastructure.Persistence;
using CraftsmenPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CraftsmenPlatform.Infrastructure.Repositories;

public class UserRepository : SoftDeletableRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> EmailExistsAsync(EmailAddress email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(u => u.Email.Value == email.Value, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.Role == role)
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(
                u => u.RefreshTokens.Any(rt => rt.Token == refreshToken),
                cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(
                u => u.Email.Value == email.Value,
                cancellationToken);
    }

    public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        await _context.Set<RefreshToken>().AddAsync(token, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Users SET LastLoginAt = {now} WHERE Id = {userId}",
            cancellationToken
        );
    }
}
