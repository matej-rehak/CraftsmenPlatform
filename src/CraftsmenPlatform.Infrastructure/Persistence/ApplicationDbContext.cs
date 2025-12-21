using Microsoft.EntityFrameworkCore;
using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Common.Interface;
using CraftsmenPlatform.Infrastructure.Events;
using System.Linq.Expressions;

namespace CraftsmenPlatform.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDomainEventDispatcher domainEventDispatcher) 
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
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
            var currentUser = GetCurrentUser();

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

    private string GetCurrentUser()
    {
        // TODO: Get from IHttpContextAccessor or ICurrentUserService
        return "System"; // Default for now
    }
}
