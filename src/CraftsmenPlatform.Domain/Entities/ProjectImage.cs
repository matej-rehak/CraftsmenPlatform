using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Exceptions;

namespace CraftsmenPlatform.Domain.Entities;

/// <summary>
/// Child entity v Project agregátu - reprezentuje obrázek projektu
/// </summary>
public class ProjectImage : BaseEntity
{
    public Guid ProjectId { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }

    // Private constructor pro EF Core
    private ProjectImage() { }

    internal ProjectImage(Guid projectId, string imageUrl, int displayOrder)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        ImageUrl = imageUrl;
        DisplayOrder = displayOrder;
        CreatedAt = DateTime.UtcNow;
    }

    public Result<ProjectImage> UpdateImageUrl(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return Result<ProjectImage>.Failure("Image URL cannot be empty");

        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;

        return Result<ProjectImage>.Success(this);
    }
}