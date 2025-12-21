using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.ValueObjects;

namespace CraftsmenPlatform.Infrastructure.Persistence.Configurations;

public class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.ToTable("Offers");

        // Primary Key
        builder.HasKey(o => o.Id);

        // Foreign keys
        builder.Property(o => o.ProjectId).IsRequired();
        builder.Property(o => o.CraftsmanId).IsRequired();

        builder.HasIndex(o => o.ProjectId)
            .HasDatabaseName("IX_Offers_ProjectId");

        builder.HasIndex(o => o.CraftsmanId)
            .HasDatabaseName("IX_Offers_CraftsmanId");

        // Composite index for querying offers by craftsman for specific project
        builder.HasIndex(o => new { o.ProjectId, o.CraftsmanId })
            .HasDatabaseName("IX_Offers_ProjectId_CraftsmanId");

        // Value Object - Price (Money)
        builder.OwnsOne(o => o.Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("PriceAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("PriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Value Object - Timeline (DateRange)
        builder.OwnsOne(o => o.Timeline, timeline =>
        {
            timeline.Property(t => t.StartDate)
                .HasColumnName("TimelineStartDate");

            timeline.Property(t => t.EndDate)
                .HasColumnName("TimelineEndDate");
        });

        // Properties
        builder.Property(o => o.Description)
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(o => o.EstimatedDurationDays);

        // Status
        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(o => o.Status)
            .HasDatabaseName("IX_Offers_Status");

        builder.Property(o => o.AcceptedAt);
        builder.Property(o => o.RejectedAt);

        // Soft Delete fields (from SoftDeletableEntity)
        builder.Property(o => o.IsDeleted).IsRequired();
        builder.Property(o => o.DeletedAt);
        builder.Property(o => o.DeletedBy).HasMaxLength(100);

        // Audit fields from BaseEntity
        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(o => o.UpdatedAt);
        builder.Property(o => o.UpdatedBy).HasMaxLength(100);

        // Concurrency token
        builder.Property(o => o.RowVersion).IsRowVersion();

        // Ignore Domain Events (not persisted)
        builder.Ignore(o => o.DomainEvents);

        // Additional indexes
        builder.HasIndex(o => o.CreatedAt)
            .HasDatabaseName("IX_Offers_CreatedAt");

        builder.HasIndex(o => o.IsDeleted)
            .HasDatabaseName("IX_Offers_IsDeleted");
    }
}
