using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Common.Interface;
using CraftsmenPlatform.Domain.Exceptions;

namespace CraftsmenPlatform.Domain.Entities;

/// <summary>
/// Agregát ChatRoom - reprezentuje chatovací místnost mezi řemeslníkem a zákazníkem
/// </summary>
public class ChatRoom : BaseEntity, IAggregateRoot
{
    public Guid ProjectId { get; private set; }
    public Guid CraftsmanId { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTime? LastMessageAt { get; private set; }

    // Messages - child entities
    private readonly List<Message> _messages = new();
    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

    public int MessageCount => _messages.Count;

    // Private constructor pro EF Core
    private ChatRoom() { }

    private ChatRoom(Guid projectId, Guid craftsmanId, Guid customerId)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        CraftsmanId = craftsmanId;
        CustomerId = customerId;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory metoda pro vytvoření nové místnosti
    /// </summary>
    public static ChatRoom Create(Guid projectId, Guid craftsmanId, Guid customerId)
    {
        return new ChatRoom(projectId, craftsmanId, customerId);
    }

    /// <summary>
    /// Odeslání zprávy
    /// </summary>
    public Result<Message> SendMessage(Guid senderId, string content)
    {
        if (senderId != CraftsmanId && senderId != CustomerId)
            return Result<Message>.Failure("Sender must be either craftsman or customer");

        if (string.IsNullOrWhiteSpace(content))
            return Result<Message>.Failure("Message content cannot be empty");

        var message = new Message(Id, senderId, content);
        _messages.Add(message);
        LastMessageAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        return Result<Message>.Success(message);
    }

    /// <summary>
    /// Označení zpráv jako přečtené
    /// </summary>
    public Result MarkMessagesAsRead(Guid userId)
    {
        if (userId != CraftsmanId && userId != CustomerId)
            return Result.Failure("User must be either craftsman or customer");

        var unreadMessages = _messages.Where(m => m.SenderId != userId && !m.IsRead).ToList();
        
        foreach (var message in unreadMessages)
        {
            message.MarkAsRead();
        }

        if (unreadMessages.Any())
            UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Počet nepřečtených zpráv pro daného uživatele
    /// </summary>
    public Result<int> GetUnreadCount(Guid userId)
    {
        return Result<int>.Success(_messages.Count(m => m.SenderId != userId && !m.IsRead));
    }
}