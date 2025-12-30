using Microsoft.EntityFrameworkCore;
using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Common.Interfaces;
using CraftsmenPlatform.Infrastructure.Events;
using System.Linq.Expressions;
using CraftsmenPlatform.Application.Common.Interfaces;

namespace CraftsmenPlatform.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDomainEventDispatcher domainEventDispatcher,
        ICurrentUserService currentUserService) 
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
        _currentUserService = currentUserService;
    }

    // DbSets - Aggregate Roots only
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<CraftsmanProfile> CraftsmanProfiles => Set<CraftsmanProfile>();
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filters for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var body = Expression.Equal(
                    Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted)),
                    Expression.Constant(false));
                var lambda = Expression.Lambda(body, parameter);

                entityType.SetQueryFilter(lambda);
            }
        }

        // -----------------------------
        // Explicit relationship configurations (no shadow FK)
        // -----------------------------

        // RefreshToken -> User (optional nav)
        modelBuilder.Entity<RefreshToken>()
            .HasOne<User>()
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .IsRequired(false);

        // CategorySkill - many-to-many between Category and Skill
        modelBuilder.Entity<CategorySkill>(entity =>
        {
            entity.HasKey(cs => new { cs.CategoryId, cs.SkillId });

            // Only Category navigation (private list in Category)
            entity.HasOne<Category>()
                  .WithMany(c => c.CategorySkills)
                  .HasForeignKey(cs => cs.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);

            // SkillId is FK-only
            entity.Property(cs => cs.SkillId).IsRequired();
        });

        // CraftsmanSkill - many-to-many between CraftsmanProfile and Skill
        modelBuilder.Entity<CraftsmanSkill>(entity =>
        {
            entity.HasKey(cs => new { cs.CraftsmanProfileId, cs.SkillId });

            entity.Property(cs => cs.CraftsmanProfileId).IsRequired();
            entity.Property(cs => cs.SkillId).IsRequired();
        });

        // Message -> ChatRoom (FK-only)
        modelBuilder.Entity<Message>()
            .HasKey(m => m.Id);

        modelBuilder.Entity<Message>()
            .Property(m => m.ChatRoomId)
            .IsRequired();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Set audit fields
        SetAuditFields();

        // Dispatch domain events BEFORE saving
        await DispatchDomainEventsAsync(cancellationToken);

        // Save changes
        var result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }

    private void SetAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            var currentUser = _currentUserService.UserId;

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetCreatedAudit(DateTime.UtcNow, currentUser);
                    break;

                case EntityState.Modified:
                    entry.Entity.SetUpdatedAudit(DateTime.UtcNow, currentUser);
                    break;

                case EntityState.Deleted:
                    if (entry.Entity is ISoftDeletable softDeletable)
                    {
                        entry.State = EntityState.Modified;
                        softDeletable.Delete(currentUser);
                    }
                    break;
            }
        }
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var domainEntities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        // Clear events from entities
        domainEntities.ForEach(entity => entity.ClearDomainEvents());

        // Dispatch events
        foreach (var domainEvent in domainEvents)
        {
            await _domainEventDispatcher.DispatchAsync(domainEvent, cancellationToken);
        }
    }
}
