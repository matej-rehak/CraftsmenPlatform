using CraftsmenPlatform.Domain.Common.Interfaces;
using CraftsmenPlatform.Domain.Repositories;
using CraftsmenPlatform.Infrastructure.Persistence;
using CraftsmenPlatform.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using CraftsmenPlatform.Application.Common.Interfaces;
using CraftsmenPlatform.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using CraftsmenPlatform.Domain.Services;
using CraftsmenPlatform.Application.Common.Settings;
using Microsoft.Extensions.Options;

namespace CraftsmenPlatform.Infrastructure;

/// <summary>
/// Extension methods pro registraci Infrastructure services
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ICraftsmanProfileRepository, CraftsmanProfileRepository>();
        services.AddScoped<ICustomerProfileRepository, CustomerProfileRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
        services.AddScoped<ISkillRepository, SkillRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // Services
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContext, HttpRequestContext>();

        services.Configure<MailjetSettings>(
            configuration.GetSection(MailjetSettings.SectionName));

        services.AddScoped<IEmailService, MailjetEmailService>();

        return services;
    }
}