using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CraftsmenPlatform.Domain.Entities;

namespace CraftsmenPlatform.Infrastructure.Persistence.Configurations;

public class CategorySkillConfiguration : IEntityTypeConfiguration<CategorySkill>
{
    public void Configure(EntityTypeBuilder<CategorySkill> builder)
    {
        builder.ToTable("CategorySkills");

        // Primary Key
        builder.HasKey(c => c.Id);

        // Foreign keys
        builder.Property(c => c.CategoryId).IsRequired();
        builder.Property(c => c.SkillId).IsRequired();

        builder.HasIndex(c => c.CategoryId)
            .HasDatabaseName("IX_CategorySkills_CategoryId");

        builder.HasIndex(c => c.SkillId)
            .HasDatabaseName("IX_CategorySkills_SkillId");

        // Composite unique index - každý skill může být v kategorii pouze jednou
        builder.HasIndex(c => new { c.CategoryId, c.SkillId })
            .IsUnique()
            .HasDatabaseName("IX_CategorySkills_CategoryId_SkillId");

        // Audit fields from BaseEntity
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(c => c.UpdatedAt);
        builder.Property(c => c.UpdatedBy).HasMaxLength(100);

        // Concurrency token
        builder.Property(c => c.RowVersion).IsRowVersion();

        // Ignore Domain Events (not persisted)
        builder.Ignore(c => c.DomainEvents);
    }
}
