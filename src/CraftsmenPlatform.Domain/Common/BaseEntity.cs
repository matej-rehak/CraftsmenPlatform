using CraftsmenPlatform.Domain.Common.Interface;

namespace CraftsmenPlatform.Domain.Common;

/// <summary>
/// Base class pro všechny entity v doméně.
/// Poskytuje základní vlastnosti jako Id, audit fields a podporu pro domain events.
/// </summary>
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Unikátní identifikátor entity
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Čas vytvoření entity
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Uživatel který vytvořil entitu
    /// </summary>
    public string CreatedBy { get; protected set; } = string.Empty;

    /// <summary>
    /// Čas poslední aktualizace
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Uživatel který naposledy aktualizoval entitu
    /// </summary>
    public string? UpdatedBy { get; protected set; } = string.Empty;

    /// <summary>
    /// Row version pro optimistic concurrency
    /// </summary>
    public byte[] RowVersion { get; protected set; } = Array.Empty<byte>();

    /// <summary>
    /// Domain events které nastaly na této entitě
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Přidání domain eventu
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Vyčištění domain events (volá se po uložení do DB)
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Equality comparison na základě Id
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (Id == Guid.Empty || other.Id == Guid.Empty)
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return (GetType().ToString() + Id).GetHashCode();
    }

    public static bool operator ==(BaseEntity? a, BaseEntity? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(BaseEntity? a, BaseEntity? b)
    {
        return !(a == b);
    }
}
