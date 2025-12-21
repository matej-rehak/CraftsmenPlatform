using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.ValueObjects;

namespace CraftsmenPlatform.Infrastructure.Persistence.Configurations;

public class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
{
    public void Configure(EntityTypeBuilder<ChatRoom> builder)
    {
        builder.ToTable("ChatRooms");

        builder.HasKey(c => c.Id);

        // Foreign keys
        builder.Property(c => c.ProjectId).IsRequired();
        builder.Property(c => c.CraftsmanId).IsRequired();
        builder.Property(c => c.CustomerId).IsRequired();

        builder.HasIndex(c => c.ProjectId)
            .HasDatabaseName("IX_ChatRooms_ProjectId");

        builder.HasIndex(c => new { c.CraftsmanId, c.CustomerId })
            .HasDatabaseName("IX_ChatRooms_CraftsmanId_CustomerId");

        // Unique constraint - one chat per project
        builder.HasIndex(c => new { c.ProjectId, c.CraftsmanId })
            .IsUnique()
            .HasDatabaseName("IX_ChatRooms_ProjectId_CraftsmanId");

        builder.Property(c => c.LastMessageAt);

        // Audit
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(c => c.UpdatedAt);
        builder.Property(c => c.UpdatedBy).HasMaxLength(100);

        builder.Property(c => c.RowVersion).IsRowVersion();
        builder.Ignore(c => c.DomainEvents);

        // Child entities - Messages
        builder.HasMany(typeof(Message))
            .WithOne()
            .HasForeignKey("ChatRoomId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}