using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Exceptions;

namespace CraftsmenPlatform.Domain.Entities;

/// <summary>
/// Child entity v ChatRoom agregátu - reprezentuje zprávu v chatu
/// </summary>
public class Message : BaseEntity
{
    public Guid ChatRoomId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTime SentAt { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    // Private constructor pro EF Core
    private Message() { }

    internal Message(Guid chatRoomId, Guid senderId, string content)
    {
        Id = Guid.NewGuid();
        ChatRoomId = chatRoomId;
        SenderId = senderId;
        Content = content?.Trim() ?? throw new ArgumentNullException(nameof(content));
        SentAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        IsRead = false;
        ReadAt = null;

        if (string.IsNullOrWhiteSpace(content))
            throw new BusinessRuleValidationException(nameof(Content), "Message content cannot be empty");

        if (content.Length > 5000)
            throw new BusinessRuleValidationException(nameof(Content), "Message content cannot exceed 5000 characters");
    }

    /// <summary>
    /// Označení zprávy jako přečtené
    /// </summary>
    internal void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}