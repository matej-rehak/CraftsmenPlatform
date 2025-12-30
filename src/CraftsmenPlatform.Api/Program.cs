using Serilog;
using Serilog.Events;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using FluentValidation;
using MediatR;
using CraftsmenPlatform.Infrastructure.Persistence;
using System.Text; // Pro Encoding
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using CraftsmenPlatform.Infrastructure.Services;
using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Common.Interfaces;
using CraftsmenPlatform.Application.Commands.Authentication.Login;
using CraftsmenPlatform.Infrastructure.Events;
using CraftsmenPlatform.Domain.Repositories;
using CraftsmenPlatform.Infrastructure.Repositories;
using CraftsmenPlatform.Application.Common.Interfaces;
using CraftsmenPlatform.Application.Common.Settings;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using CraftsmenPlatform.Infrastructure.Services;
using CraftsmenPlatform.Domain.Services;


var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();    

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHttpContextAccessor();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();

// Add UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add RequestContext
builder.Services.AddScoped<IRequestContext, HttpRequestContext>();

// Add CurrentUserService
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Registrace MediatR a FluentValidation
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(LoginCommand).Assembly));

builder.Services.AddValidatorsFromAssembly(typeof(LoginCommand).Assembly);
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Authentication services
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
builder.Services.AddScoped<IEmailService, MailjetEmailService>();

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Rejection response
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    // 1. Global IP-based Protection (Fixed Window)
    options.AddFixedWindowLimiter("global", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });
    
    // 2. Authentication Endpoints (Sliding Window) - brute-force protection
    options.AddSlidingWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 2;
        opt.QueueLimit = 2;
    });
    
    // 3. Per-User Rate Limiting (Token Bucket)
    options.AddPolicy("per-user", context =>
    {
        var userId = context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value 
                     ?? "anonymous";
        
        return RateLimitPartition.GetTokenBucketLimiter(userId, _ =>
            new TokenBucketRateLimiterOptions
            {
                TokenLimit = 30,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                TokensPerPeriod = 30,
                AutoReplenishment = true
            });
    });
    
    // 4. Resource-Intensive Operations (Concurrency Limiter)
    options.AddConcurrencyLimiter("concurrent", opt =>
    {
        opt.PermitLimit = 3;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });
    
    // Custom rejection handler
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        
        var retryAfter = 60; // default
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterMetadata))
        {
            retryAfter = (int)retryAfterMetadata.TotalSeconds;
            context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();
        }
        
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests",
            message = "Rate limit exceeded. Please try again later.",
            retryAfter = retryAfter
        }, cancellationToken);
    };
});

// JWT Authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false; // Set to true in production
        var key = builder.Configuration["JwtSettings:Secret"];
        if (string.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException("JWT Key není nastaven v konfiguraci.");
        }
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key)),
            ClockSkew = TimeSpan.Zero // Remove delay of token expiration
        };

        // Events for debugging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(
                    new { error = "You are not authorized" });
                return context.Response.WriteAsync(result);
            }
        };
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    // Role-based policies
    options.AddPolicy("RequireAdminRole", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("RequireCraftsmanRole", policy => 
        policy.RequireRole("Craftsman", "Admin"));
    
    options.AddPolicy("RequireCustomerRole", policy => 
        policy.RequireRole("Customer", "Admin"));

    // Email verified policy
    options.AddPolicy("RequireVerifiedEmail", policy =>
        policy.RequireClaim("email_verified", "True"));
});

// česky - umožňuje všechny požadavky

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// česky - loguje všechny požadavky

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    // česky - vytvori tabulky v DB
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    
    // Seed database with sample data (development only)
    if (app.Environment.IsDevelopment())
    {
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var seeder = new DatabaseSeeder(db, passwordHasher);
        await seeder.SeedAsync();
        
        Log.Information("Database seeded with sample data");
    }
}

app.Run();
