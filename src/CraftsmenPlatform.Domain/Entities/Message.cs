public class Message : SoftDeletableEntity
{
    public Guid ChatRoomId { get; private set; }
    public ChatRoom ChatRoom { get; private set; } = null!;

    public Guid SenderId { get; private set; }
    public User Sender { get; private set; } = null!;

    public string Content { get; private set; } = string.Empty;
    public DateTime SentAt { get; private set; }

    public bool IsRead { get; private set; }
}