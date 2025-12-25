using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.ValueObjects;

namespace CraftsmenPlatform.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        // Primary Key
        builder.HasKey(p => p.Id);

        // Foreign key - CustomerId
        builder.Property(p => p.CustomerId).IsRequired();

        builder.HasIndex(p => p.CustomerId)
            .HasDatabaseName("IX_Projects_CustomerId");

        // Basic Properties
        builder.Property(p => p.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(5000)
            .IsRequired();

        // Value Objects - Budget (Money)
        builder.OwnsOne(p => p.BudgetMin, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("BudgetMinAmount")
                .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                .HasColumnName("BudgetMinCurrency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(p => p.BudgetMax, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("BudgetMaxAmount")
                .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                .HasColumnName("BudgetMaxCurrency")
                .HasMaxLength(3);
        });

        // Timeline dates
        builder.Property(p => p.PreferredStartDate);
        builder.Property(p => p.Deadline);

        // Status
        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Projects_Status");

        builder.Property(p => p.PublishedAt);
        builder.Property(p => p.CompletedAt);

        // Accepted Offer
        builder.Property(p => p.AcceptedOfferId);

        // Child entities - Offers
        builder.HasMany(p => p.Offers)
            .WithOne()
            .HasForeignKey("ProjectId") // Offer has ProjectId property but no Project navigation
            .OnDelete(DeleteBehavior.Cascade);

        // Child entities - Images
        builder.HasMany(p => p.Images)
            .WithOne()
            .HasForeignKey("ProjectId") // ProjectImage has ProjectId property but no Project navigation
            .OnDelete(DeleteBehavior.Cascade);

        // Soft Delete fields (from SoftDeletableEntity)
        builder.Property(p => p.IsDeleted).IsRequired();
        builder.Property(p => p.DeletedAt);
        builder.Property(p => p.DeletedBy).HasMaxLength(100);

        // Audit fields from BaseEntity
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(p => p.UpdatedAt);
        builder.Property(p => p.UpdatedBy).HasMaxLength(100);

        // Concurrency token
        builder.Property(p => p.RowVersion).IsRowVersion();

        // Ignore Domain Events (not persisted)
        builder.Ignore(p => p.DomainEvents);

        // Additional indexes for querying
        builder.HasIndex(p => p.PublishedAt)
            .HasDatabaseName("IX_Projects_PublishedAt");

        builder.HasIndex(p => p.CreatedAt)
            .HasDatabaseName("IX_Projects_CreatedAt");

        // Index for soft delete queries (already handled by global query filter)
        builder.HasIndex(p => p.IsDeleted)
            .HasDatabaseName("IX_Projects_IsDeleted");
    }
}
