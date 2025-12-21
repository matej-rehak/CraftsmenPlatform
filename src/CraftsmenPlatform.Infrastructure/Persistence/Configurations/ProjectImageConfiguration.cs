using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CraftsmenPlatform.Domain.Entities;

namespace CraftsmenPlatform.Infrastructure.Persistence.Configurations;

public class ProjectImageConfiguration : IEntityTypeConfiguration<ProjectImage>
{
    public void Configure(EntityTypeBuilder<ProjectImage> builder)
    {
        builder.ToTable("ProjectImages");

        // Primary Key
        builder.HasKey(p => p.Id);

        // Foreign key
        builder.Property(p => p.ProjectId).IsRequired();

        builder.HasIndex(p => p.ProjectId)
            .HasDatabaseName("IX_ProjectImages_ProjectId");

        // Properties
        builder.Property(p => p.ImageUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.DisplayOrder).IsRequired();

        // Composite index for ordering images within a project
        builder.HasIndex(p => new { p.ProjectId, p.DisplayOrder })
            .HasDatabaseName("IX_ProjectImages_ProjectId_DisplayOrder");

        // Audit fields from BaseEntity
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(p => p.UpdatedAt);
        builder.Property(p => p.UpdatedBy).HasMaxLength(100);

        // Concurrency token
        builder.Property(p => p.RowVersion).IsRowVersion();

        // Ignore Domain Events (not persisted)
        builder.Ignore(p => p.DomainEvents);
    }
}
