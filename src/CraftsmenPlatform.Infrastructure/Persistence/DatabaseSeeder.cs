using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.Enums;
using CraftsmenPlatform.Domain.ValueObjects;
using CraftsmenPlatform.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using CraftsmenPlatform.Application.Common.Interfaces;

namespace CraftsmenPlatform.Infrastructure.Persistence;

/// <summary>
/// Seeds database with sample data for development environment
/// </summary>
public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseSeeder(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        // Check if database is already seeded
        if (await _context.Users.AnyAsync())
        {
            return; // Database already seeded
        }

        await SeedUsersAsync();
        await SeedCategoriesAndSkillsAsync();
        await SeedProjectsAsync();
        
        await _context.SaveChangesAsync();
    }

    private async Task SeedUsersAsync()
    {
        var users = new List<User>();

        // 1. Admin User
        var adminResult = User.CreateUser(
            "admin@craftsmenplatform.cz",
            _passwordHasher.HashPassword("Admin123!"),
            "Admin",
            "System"
        );

        if (adminResult.IsSuccess)
        {
            var admin = adminResult.Value;
            admin.ChangeRole(UserRole.Admin);
            admin.VerifyEmail();
            users.Add(admin);
        }

        // 2. Test Customers
        var customer1Result = User.CreateCustomer(
            "jan.novak@email.cz",
            _passwordHasher.HashPassword("Password123!"),
            "Jan",
            "Novák"
        );
        if (customer1Result.IsSuccess)
        {
            customer1Result.Value.VerifyEmail();
            users.Add(customer1Result.Value);
        }

        var customer2Result = User.CreateCustomer(
            "petra.svobodova@email.cz",
            _passwordHasher.HashPassword("Password123!"),
            "Petra",
            "Svobodová"
        );
        if (customer2Result.IsSuccess)
        {
            customer2Result.Value.VerifyEmail();
            users.Add(customer2Result.Value);
        }

        // 3. Test Craftsmen
        var craftsman1Result = User.CreateCraftsman(
            "karel.stavitel@email.cz",
            _passwordHasher.HashPassword("Password123!"),
            "Karel",
            "Stavitel"
        );
        if (craftsman1Result.IsSuccess)
        {
            craftsman1Result.Value.VerifyEmail();
            users.Add(craftsman1Result.Value);
        }

        var craftsman2Result = User.CreateCraftsman(
            "tomas.elektrikar@email.cz",
            _passwordHasher.HashPassword("Password123!"),
            "Tomáš",
            "Elektrikář"
        );
        if (craftsman2Result.IsSuccess)
        {
            craftsman2Result.Value.VerifyEmail();
            users.Add(craftsman2Result.Value);
        }

        var craftsman3Result = User.CreateCraftsman(
            "milan.instalater@email.cz",
            _passwordHasher.HashPassword("Password123!"),
            "Milan",
            "Instalatér"
        );
        if (craftsman3Result.IsSuccess)
        {
            craftsman3Result.Value.VerifyEmail();
            users.Add(craftsman3Result.Value);
        }

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync(); // Save to get IDs
    }

    private async Task SeedCategoriesAndSkillsAsync()
    {
        var skills = new List<Skill>();
        
        // First, create all skills
        var zednictvi = Skill.Create("Zednictví", "Zdění, omítání").Value;
        var betonaz = Skill.Create("Betonáž", "Betonové práce").Value;
        var sadrokarton = Skill.Create("Sádrokarton", "Sádrokartonové konstrukce").Value;
        var elektroinstalace = Skill.Create("Elektroinstalace", "Instalace el. rozvodů").Value;
        var revizeElektro = Skill.Create("Revize elektro", "Revize elektro rozvodů").Value;
        var vodoinstalace = Skill.Create("Vodoinstalace", "Instalace vody a kanalizace").Value;
        var topeni = Skill.Create("Topení", "Instalace a údržba topení").Value;
        var malovani = Skill.Create("Malování", "Malování interiérů a exteriérů").Value;
        var tapetovani = Skill.Create("Tapetování", "Pokládka tapet").Value;

        skills.AddRange(new[] { zednictvi, betonaz, sadrokarton, elektroinstalace, 
                                revizeElektro, vodoinstalace, topeni, malovani, tapetovani });

        // Save skills first so they get IDs
        await _context.Skills.AddRangeAsync(skills);
        await _context.SaveChangesAsync();

        // Now create categories and add skill relationships
        var categories = new List<Category>();

        // 1. Stavební práce
        var stavebniResult = Category.Create(
            "Stavební práce",
            "Kompletní stavební služby",
            "/icons/construction.svg"
        );
        if (stavebniResult.IsSuccess)
        {
            var category = stavebniResult.Value;
            category.AddSkill(zednictvi.Id);
            category.AddSkill(betonaz.Id);
            category.AddSkill(sadrokarton.Id);
            categories.Add(category);
        }

        // 2. Elektrikářské práce
        var elektroResult = Category.Create(
            "Elektrika",
            "Elektroinstalatérské služby",
            "/icons/electric.svg"
        );
        if (elektroResult.IsSuccess)
        {
            var category = elektroResult.Value;
            category.AddSkill(elektroinstalace.Id);
            category.AddSkill(revizeElektro.Id);
            categories.Add(category);
        }

        // 3. Instalatérské práce
        var instalaResult = Category.Create(
            "Instalatérství",
            "Vodoinstalace a topení",
            "/icons/plumbing.svg"
        );
        if (instalaResult.IsSuccess)
        {
            var category = instalaResult.Value;
            category.AddSkill(vodoinstalace.Id);
            category.AddSkill(topeni.Id);
            categories.Add(category);
        }

        // 4. Malířské práce
        var malirResult = Category.Create(
            "Malířství",
            "Malířské a natěračské práce",
            "/icons/paint.svg"
        );
        if (malirResult.IsSuccess)
        {
            var category = malirResult.Value;
            category.AddSkill(malovani.Id);
            category.AddSkill(tapetovani.Id);
            categories.Add(category);
        }

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();
    }

    private async Task SeedProjectsAsync()
    {
        var customer1 = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == "jan.novak@email.cz");
        
        var customer2 = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == "petra.svobodova@email.cz");

        if (customer1 == null || customer2 == null)
            return;

        var projects = new List<Project>();

        // Project 1 - Published
        var addressResult1 = Address.Create("Hlavní 123", "Praha", "Praha", "11000", "Česká republika");
        var budgetMin1 = Money.Create(50000, "CZK");
        var budgetMax1 = Money.Create(80000, "CZK");

        if (addressResult1.IsSuccess && budgetMin1.IsSuccess && budgetMax1.IsSuccess)
        {
            var project1Result = Project.Create(
                customer1.Id,
                "Rekonstrukce koupelny",
                "Kompletní rekonstrukce koupelny v panelovém bytě. Zahrnuje bourací práce, nové obklady, instalace, elektrika.",
                budgetMin1.Value,
                budgetMax1.Value,
                DateTime.UtcNow.AddDays(14),
                DateTime.UtcNow.AddDays(60)
            );

            if (project1Result.IsSuccess)
            {
                var project = project1Result.Value;
                project.Publish();
                projects.Add(project);
            }
        }

        // Project 2 - Published
        var addressResult2 = Address.Create("Dlouhá 45", "Brno", "Jihomoravský", "60200", "Česká republika");
        var budgetMin2 = Money.Create(30000, "CZK");
        var budgetMax2 = Money.Create(45000, "CZK");

        if (addressResult2.IsSuccess && budgetMin2.IsSuccess && budgetMax2.IsSuccess)
        {
            var project2Result = Project.Create(
                customer1.Id,
                "Malování bytu",
                "Malování 3+1 bytové jednotky, cca 75m². Všechny místnosti kromě koupelny a WC.",
                budgetMin2.Value,
                budgetMax2.Value,
                DateTime.UtcNow.AddDays(7),
                DateTime.UtcNow.AddDays(21)
            );

            if (project2Result.IsSuccess)
            {
                var project = project2Result.Value;
                project.Publish();
                projects.Add(project);
            }
        }

        // Project 3 - Draft
        var addressResult3 = Address.Create("Krátká 8", "Ostrava", "Moravskoslezský", "70200", "Česká republika");
        var budgetMin3 = Money.Create(100000, "CZK");
        var budgetMax3 = Money.Create(150000, "CZK");

        if (budgetMin3.IsSuccess && budgetMax3.IsSuccess)
        {
            var project3Result = Project.Create(
                customer2.Id,
                "Rekonstrukce elektroinstalace",
                "Kompletní výměna elektroinstalace v rodinném domě včetně nového rozvaděče.",
                budgetMin3.Value,
                budgetMax3.Value,
                null,
                DateTime.UtcNow.AddDays(90)
            );

            if (project3Result.IsSuccess)
            {
                // Stays in Draft
                projects.Add(project3Result.Value);
            }
        }

        // Project 4 - Published with offers
        var addressResult4 = Address.Create("Nová 67", "Plzeň", "Plzeňský", "30100", "Česká republika");
        var budgetMin4 = Money.Create(20000, "CZK");
        var budgetMax4 = Money.Create(35000, "CZK");

        if (budgetMin4.IsSuccess && budgetMax4.IsSuccess)
        {
            var project4Result = Project.Create(
                customer2.Id,
                "Instalace nového topení",
                "Instalace radiátorů v 2 pokojích + připojení na kotel.",
                budgetMin4.Value,
                budgetMax4.Value,
                DateTime.UtcNow.AddDays(10),
                DateTime.UtcNow.AddDays(30)
            );

            if (project4Result.IsSuccess)
            {
                var project = project4Result.Value;
                project.Publish();
                projects.Add(project);
            }
        }

        await _context.Projects.AddRangeAsync(projects);
        await _context.SaveChangesAsync();
    }
}
