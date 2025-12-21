using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.ValueObjects;

namespace CraftsmenPlatform.Infrastructure.Persistence.Configurations;

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("Skills");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(s => s.Name)
            .IsUnique()
            .HasDatabaseName("IX_Skills_Name");

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.IconUrl)
            .HasMaxLength(500);

        // Child entities - CategorySkills (with backing field)
        builder.HasMany(typeof(CategorySkill))
            .WithOne()
            .HasForeignKey("SkillId")
            .OnDelete(DeleteBehavior.Cascade);

        // Audit
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(s => s.UpdatedAt);
        builder.Property(s => s.UpdatedBy).HasMaxLength(100);

        builder.Property(s => s.RowVersion).IsRowVersion();
        builder.Ignore(s => s.DomainEvents);

        builder.HasIndex(s => s.CreatedAt)
            .HasDatabaseName("IX_Skills_CreatedAt");
    }
}
