using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.ValueObjects;

namespace CraftsmenPlatform.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);

        // Foreign keys
        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.ProjectId).IsRequired();
        builder.Property(r => r.CraftsmanId).IsRequired();

        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("IX_Reviews_UserId");

        builder.HasIndex(r => r.ProjectId)
            .IsUnique()
            .HasDatabaseName("IX_Reviews_ProjectId");

        builder.HasIndex(r => r.CraftsmanId)
            .HasDatabaseName("IX_Reviews_CraftsmanId");

        // Rating - Value Object
        builder.OwnsOne(r => r.Rating, rating =>
        {
            rating.Property(rt => rt.Value)
                .HasColumnName("Rating")
                .HasPrecision(3, 1)
                .IsRequired();
        });

        builder.Property(r => r.Comment)
            .HasMaxLength(2000);

        builder.Property(r => r.IsPublished).IsRequired();
        builder.Property(r => r.IsVerified).IsRequired();

        builder.Property(r => r.PublishedAt);
        builder.Property(r => r.VerifiedAt);

        // Audit
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(r => r.UpdatedAt);
        builder.Property(r => r.UpdatedBy).HasMaxLength(100);

        builder.Property(r => r.RowVersion).IsRowVersion();
        builder.Ignore(r => r.DomainEvents);

        // Indexes
        builder.HasIndex(r => r.IsPublished)
            .HasDatabaseName("IX_Reviews_IsPublished");

        builder.HasIndex(r => r.PublishedAt)
            .HasDatabaseName("IX_Reviews_PublishedAt");
    }
}