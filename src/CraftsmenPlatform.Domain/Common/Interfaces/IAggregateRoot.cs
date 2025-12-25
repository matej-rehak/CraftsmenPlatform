namespace CraftsmenPlatform.Domain.Common.Interfaces;

/// <summary>
/// Marker interface pro identifikaci Aggregate Roots.
/// Aggregate Root je hlavní entita agregátu, která garantuje konzistenci
/// všech entit v rámci agregátu a je jediným vstupním bodem pro změny.
/// </summary>
public interface IAggregateRoot
{
}
