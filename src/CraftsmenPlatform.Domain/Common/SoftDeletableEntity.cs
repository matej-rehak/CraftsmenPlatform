using CraftsmenPlatform.Domain.Common.Interface;

namespace CraftsmenPlatform.Domain.Common;

/// <summary>
/// Base class pro entity které podporují soft delete
/// </summary>
public abstract class SoftDeletableEntity : BaseEntity, ISoftDeletable
{
    public bool IsDeleted { get; protected set; } = false;
    public DateTime? DeletedAt { get; protected set; }
    public string? DeletedBy { get; protected set; }

    /// <summary>
    /// Soft delete entity
    /// </summary>
    public virtual void Delete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    /// <summary>
    /// Restore soft deleted entity
    /// </summary>
    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}