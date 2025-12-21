using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.ValueObjects;

namespace CraftsmenPlatform.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(m => m.Id);

        // Foreign keys
        builder.Property(m => m.ChatRoomId).IsRequired();
        builder.Property(m => m.SenderId).IsRequired();

        builder.HasIndex(m => m.ChatRoomId)
            .HasDatabaseName("IX_Messages_ChatRoomId");

        builder.HasIndex(m => m.SenderId)
            .HasDatabaseName("IX_Messages_SenderId");

        builder.Property(m => m.Content)
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(m => m.IsRead).IsRequired();
        builder.Property(m => m.ReadAt);

        // Audit
        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(m => m.UpdatedAt);
        builder.Property(m => m.UpdatedBy).HasMaxLength(100);

        builder.Property(m => m.RowVersion).IsRowVersion();
        builder.Ignore(m => m.DomainEvents);

        // Index for querying unread messages
        builder.HasIndex(m => new { m.ChatRoomId, m.IsRead })
            .HasDatabaseName("IX_Messages_ChatRoomId_IsRead");

        builder.HasIndex(m => m.CreatedAt)
            .HasDatabaseName("IX_Messages_CreatedAt");
    }
}