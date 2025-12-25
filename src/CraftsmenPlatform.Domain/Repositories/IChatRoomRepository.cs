using CraftsmenPlatform.Domain.Entities;

namespace CraftsmenPlatform.Domain.Repositories;

public interface IChatRoomRepository : ISoftDeletableRepository<ChatRoom>
{
    /// <summary>
    /// Získá chat room podle projektu a účastníků
    /// </summary>
    Task<ChatRoom?> GetByProjectAndParticipantsAsync(
        Guid projectId, 
        Guid craftsmanId, 
        Guid customerId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Získá chat rooms uživatele
    /// </summary>
    Task<IReadOnlyList<ChatRoom>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Získá chat room včetně zpráv
    /// </summary>
    Task<ChatRoom?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Získá chat rooms s nepřečtenými zprávami pro uživatele
    /// </summary>
    Task<IReadOnlyList<ChatRoom>> GetWithUnreadMessagesAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);
}