using Microsoft.EntityFrameworkCore;
using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.Common;

namespace CraftsmenPlatform.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
}
