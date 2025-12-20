using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using CraftsmenPlatform.Domain.Entities;

namespace CraftsmenPlatform.Infrastructure.Data
{
    public class CraftsmenPlatformDbContext : DbContext
    {
        public CraftsmenPlatformDbContext(DbContextOptions<CraftsmenPlatformDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Offer> Offers => Set<Offer>();
        public DbSet<ProjectImage> ProjectImages => Set<ProjectImage>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Craftsman> Craftsman => Set<Craftsman>();
    }
}