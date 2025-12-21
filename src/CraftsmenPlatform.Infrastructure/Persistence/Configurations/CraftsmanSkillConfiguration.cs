using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CraftsmenPlatform.Domain.Entities;

namespace CraftsmenPlatform.Infrastructure.Persistence.Configurations;

public class CraftsmanSkillConfiguration : IEntityTypeConfiguration<CraftsmanSkill>
{
    public void Configure(EntityTypeBuilder<CraftsmanSkill> builder)
    {
        builder.ToTable("CraftsmanSkills");

        // Primary Key
        builder.HasKey(c => c.Id);

        // Foreign keys
        builder.Property(c => c.CraftsmanProfileId).IsRequired();
        builder.Property(c => c.SkillId).IsRequired();

        builder.HasIndex(c => c.CraftsmanProfileId)
            .HasDatabaseName("IX_CraftsmanSkills_CraftsmanProfileId");

        builder.HasIndex(c => c.SkillId)
            .HasDatabaseName("IX_CraftsmanSkills_SkillId");

        // Composite unique index - one skill per craftsman profile
        // This enforces the business rule: "Skill lze přidat pouze jednou"
        builder.HasIndex(c => new { c.CraftsmanProfileId, c.SkillId })
            .IsUnique()
            .HasDatabaseName("IX_CraftsmanSkills_CraftsmanProfileId_SkillId");

        // Properties
        builder.Property(c => c.YearsOfExperience);

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
