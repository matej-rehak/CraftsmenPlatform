using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Common.Interfaces;

namespace CraftsmenPlatform.Domain.Entities;

/// <summary>
/// Agregát CustomerProfile - reprezentuje profil zákazníka
/// </summary>
public class CustomerProfile : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }

    // Statistiky
    public int TotalProjectsCreated { get; private set; }
    public int CompletedProjects { get; private set; }

    // Private constructor pro EF Core  
    private CustomerProfile() { }

    private CustomerProfile(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        TotalProjectsCreated = 0;
        CompletedProjects = 0;
    }

    /// <summary>
    /// Factory metoda pro vytvoření nového customer profilu
    /// </summary>
    public static CustomerProfile Create(Guid userId)
    {
        return new CustomerProfile(userId);
    }

    /// <summary>
    /// Inkrementování vytvořených projektů
    /// </summary>
    internal void IncrementProjectsCreated()
    {
        TotalProjectsCreated++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Inkrementování dokončených projektů
    /// </summary>
    internal void IncrementCompletedProjects()
    {
        CompletedProjects++;
        UpdatedAt = DateTime.UtcNow;
    }
}