namespace CraftsmenPlatform.Domain.Common.Interface;

/// <summary>
/// Interface pro entity podporující soft delete
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }
    string? DeletedBy { get; }
    void Delete(string deletedBy);
    void Restore();
}