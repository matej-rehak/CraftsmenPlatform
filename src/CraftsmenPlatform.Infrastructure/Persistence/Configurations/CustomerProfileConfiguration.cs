using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.ValueObjects;

namespace CraftsmenPlatform.Infrastructure.Persistence.Configurations;

public class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder.ToTable("CustomerProfiles");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId).IsRequired();

        builder.HasIndex(c => c.UserId)
            .IsUnique()
            .HasDatabaseName("IX_CustomerProfiles_UserId");

        builder.Property(c => c.TotalProjectsCreated).IsRequired();
        builder.Property(c => c.CompletedProjects).IsRequired();

        // Audit
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(c => c.UpdatedAt);
        builder.Property(c => c.UpdatedBy).HasMaxLength(100);

        builder.Property(c => c.RowVersion).IsRowVersion();
        builder.Ignore(c => c.DomainEvents);
    }
}