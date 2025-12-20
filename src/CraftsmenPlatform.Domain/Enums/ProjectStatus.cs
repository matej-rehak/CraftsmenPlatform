namespace CraftsmenPlatform.Domain.Enums;

public enum ProjectStatus
{
    /// <summary>
    /// Koncept - zákazník ještě nedokončil
    /// </summary>
    Draft = 1,
    
    /// <summary>
    /// Publikovaný - řemeslníci mohou dávat nabídky
    /// </summary>
    Published = 2,
    
    /// <summary>
    /// V realizaci - přijatá nabídka, probíhá práce
    /// </summary>
    InProgress = 3,
    
    /// <summary>
    /// Dokončený - hotovo
    /// </summary>
    Completed = 4,
    
    /// <summary>
    /// Zrušený
    /// </summary>
    Cancelled = 5,
    
    /// <summary>
    /// Zavřený - automaticky po čase bez nabídek
    /// </summary>
    Closed = 6
}