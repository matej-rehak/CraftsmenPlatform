using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.ValueObjects;

namespace CraftsmenPlatform.Infrastructure.Persistence.Configurations;

public class CraftsmanProfileConfiguration : IEntityTypeConfiguration<CraftsmanProfile>
{
    public void Configure(EntityTypeBuilder<CraftsmanProfile> builder)
    {
        builder.ToTable("CraftsmanProfiles");

        // Primary Key
        builder.HasKey(c => c.Id);

        // Foreign key - UserId (one-to-one relationship)
        builder.Property(c => c.UserId).IsRequired();

        builder.HasIndex(c => c.UserId)
            .IsUnique()
            .HasDatabaseName("IX_CraftsmanProfiles_UserId");

        // Basic Properties
        builder.Property(c => c.Bio)
            .HasMaxLength(2000);

        builder.Property(c => c.RegistrationNumber)
            .HasMaxLength(50);

        builder.Property(c => c.TaxNumber)
            .HasMaxLength(50);

        builder.Property(c => c.YearsOfExperience);

        // Rating - Note: AverageRating is stored as decimal, not as Rating value object
        // because it's calculated dynamically from reviews
        builder.Property(c => c.AverageRating)
            .HasPrecision(3, 1)
            .IsRequired();

        builder.Property(c => c.TotalReviews).IsRequired();
        builder.Property(c => c.CompletedProjectsCount).IsRequired();

        // Status
        builder.Property(c => c.IsVerified).IsRequired();
        builder.Property(c => c.VerifiedAt);
        builder.Property(c => c.IsAvailable).IsRequired();

        // Child entities - Skills (with backing field)
        builder.HasMany(typeof(CraftsmanSkill))
            .WithOne()
            .HasForeignKey("CraftsmanProfileId")
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

        // Indexes for querying
        builder.HasIndex(c => c.IsVerified)
            .HasDatabaseName("IX_CraftsmanProfiles_IsVerified");

        builder.HasIndex(c => c.IsAvailable)
            .HasDatabaseName("IX_CraftsmanProfiles_IsAvailable");

        builder.HasIndex(c => c.AverageRating)
            .HasDatabaseName("IX_CraftsmanProfiles_AverageRating");

        builder.HasIndex(c => c.CreatedAt)
            .HasDatabaseName("IX_CraftsmanProfiles_CreatedAt");
    }
}
