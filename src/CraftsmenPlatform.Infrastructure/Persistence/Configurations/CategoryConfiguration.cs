using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CraftsmenPlatform.Domain.Entities;

namespace CraftsmenPlatform.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        // Primary Key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasDatabaseName("IX_Categories_Name");

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.IconUrl)
            .HasMaxLength(500);

        builder.Property(c => c.IsActive).IsRequired();

        // Child entities - CategorySkills (with backing field)
        builder.HasMany(typeof(CategorySkill))
            .WithOne()
            .HasForeignKey("CategoryId")
            .OnDelete(DeleteBehavior.Cascade);

        // Audit fields from BaseEntity
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(c => c.UpdatedAt);
        builder.Property(c => c.UpdatedBy).HasMaxLength(100);

        // Concurrency token
        builder.Property(c => c.RowVersion).IsRowVersion();

        // Ignore Domain Events (not persisted)
        builder.Ignore(c => c.DomainEvents);

        // Indexes
        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Categories_IsActive");

        builder.HasIndex(c => c.CreatedAt)
            .HasDatabaseName("IX_Categories_CreatedAt");
    }
}
