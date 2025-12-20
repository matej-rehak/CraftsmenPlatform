namespace CraftsmenPlatform.Domain.Enums;

public enum OfferStatus
{
    /// <summary>
    /// Čeká na rozhodnutí zákazníka
    /// </summary>
    Pending = 1,
    
    /// <summary>
    /// Přijatá zákazníkem
    /// </summary>
    Accepted = 2,
    
    /// <summary>
    /// Odmítnutá zákazníkem
    /// </summary>
    Rejected = 3,
    
    /// <summary>
    /// Stažená řemeslníkem
    /// </summary>
    Withdrawn = 4,
    
    /// <summary>
    /// Automaticky odmítnutá (zákazník přijal jinou nabídku)
    /// </summary>
    AutoRejected = 5
}