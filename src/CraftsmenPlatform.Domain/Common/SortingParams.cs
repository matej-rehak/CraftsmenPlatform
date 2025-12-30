namespace CraftsmenPlatform.Domain.Common;

/// <summary>
/// Sorting parameters
/// </summary>
public class SortingParams
{
    public string? OrderBy { get; set; }
    public bool IsDescending { get; set; }

    /// <summary>
    /// Parse from query string format: "createdAt:desc" or "title:asc"
    /// </summary>
    public static SortingParams Parse(string? sortQuery)
    {
        if (string.IsNullOrWhiteSpace(sortQuery))
            return new SortingParams();

        var parts = sortQuery.Split(':');
        return new SortingParams
        {
            OrderBy = parts[0],
            IsDescending = parts.Length > 1 && 
                          parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase)
        };
    }
}