using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.Repositories;
using CraftsmenPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CraftsmenPlatform.Infrastructure.Repositories;

public class ChatRoomRepository : SoftDeletableRepository<ChatRoom>, IChatRoomRepository
{
    public ChatRoomRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ChatRoom?> GetByProjectAndParticipantsAsync(
        Guid projectId,
        Guid craftsmanId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(cr => 
                cr.ProjectId == projectId && 
                cr.CraftsmanId == craftsmanId && 
                cr.CustomerId == customerId, 
                cancellationToken);
    }

    public async Task<IReadOnlyList<ChatRoom>> GetByUserIdAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cr => cr.CraftsmanId == userId || cr.CustomerId == userId)
            .OrderByDescending(cr => cr.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatRoom?> GetByIdWithMessagesAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(cr => cr.Messages.OrderBy(m => m.SentAt))
            .FirstOrDefaultAsync(cr => cr.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ChatRoom>> GetWithUnreadMessagesAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(cr => cr.Messages)
            .Where(cr => 
                (cr.CraftsmanId == userId || cr.CustomerId == userId) &&
                cr.Messages.Any(m => m.SenderId != userId && !m.IsRead))
            .OrderByDescending(cr => cr.UpdatedAt)
            .ToListAsync(cancellationToken);
    }
}