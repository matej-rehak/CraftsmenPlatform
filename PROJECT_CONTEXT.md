# CraftsmenPlatform - AI Agent Context Documentation

> 📋 **Účel dokumentu**: Tento dokument slouží k udržení kontextu pro AI agenty, poskytuje kompletní přehled projektu, jeho architektury a klíčových rozhodnutí.

## 📖 Přehled Projektu

**CraftsmenPlatform** je platforma spojující řemeslníky se zákazníky. Umožňuje zákazníkům vytvářet projekty, řemeslníkům na ně nabízet, komunikovat a hodnotit dokončené práce.

### Klíčové Funkce
- 📋 Vytváření a správa projektů zákazníky
- 💼 Profily řemeslníků s dovednostmi a hodnocením
- 💰 Systém nabídek a akceptování nabídek
- 💬 Chat mezi řemeslníky a zákazníky
- ⭐ Hodnocení a recenze

### 🛠️ Technology Stack

| Technology | Version | Usage |
|------------|---------|-------|
| **.NET SDK** | 8.0 | Core Framework |
| **Entity Framework Core** | 8.0.6 | ORM & Database Access |
| **MediatR** | 14.0.0 | Mediator Pattern, CQRS, Domain Events |
| **FluentValidation** | 12.1.1 | Validation Logic |
| **Serilog** | 10.0.0 | Logging |
| **Swashbuckle (Swagger)** | 6.6.2 | API Documentation |

### Error Handling Strategy

Projekt používá **Pure Result Pattern** pro veškerou business a domain logiku:

#### Result Pattern (Pro Vše v Domain Layer)

**Všechny** domain operace, včetně vytváření entit a value objects, používají **Result pattern**:

```csharp
// Business operace - Result
public Result AcceptOffer(Guid offerId)
{
    if (Status != ProjectStatus.Published)
        return Result.Failure("Cannot accept offer for non-published project");
    
    // ... business logic
    return Result.Success();
}

// Factory metody - Result
public static Result<Project> Create(string title, string description, ...)
{
    if (string.IsNullOrWhiteSpace(title))
        return Result<Project>.Failure("Title cannot be empty");
    
    if (string.IsNullOrWhiteSpace(description))
        return Result<Project>.Failure("Description cannot be empty");
    
    var project = new Project(title, description, ...);
    return Result<Project>.Success(project);
}

// Private constructor - bez validace (validace je v Create)
private Project(string title, string description, ...)
{
    Title = title;
    Description = description;
    // ... další inicializace
}

// Value Objects - Result
public static Result<EmailAddress> Create(string value)
{
    if (string.IsNullOrWhiteSpace(value))
        return Result<EmailAddress>.Failure("Email cannot be empty");
    
    if (!IsValidEmail(value))
        return Result<EmailAddress>.Failure("Invalid email format");
    
    return Result<EmailAddress>.Success(new EmailAddress(value));
}
```

**Použití v Application Layer:**
```csharp
// Command Handler
public async Task<Result> Handle(CreateProjectCommand request, CancellationToken ct)
{
    // Vytvoření value objects
    var addressResult = Address.Create(request.Street, request.City, ...);
    if (addressResult.IsFailure)
        return Result.Failure(addressResult.Error);
    
    // Vytvoření aggregate
    var projectResult = Project.Create(
        request.Title, 
        request.Description, 
        addressResult.Value,
        ...
    );
    
    if (projectResult.IsFailure)
        return Result.Failure(projectResult.Error);
    
    await _repository.AddAsync(projectResult.Value);
    await _unitOfWork.SaveChangesAsync(ct);
    
    return Result.Success();
}
```
**Použití v API Layer:**
```csharp
[HttpPost]
public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
{
    var command = new CreateProjectCommand(...);
    var result = await _mediator.Send(command);
    
    if (result.IsFailure)
        return BadRequest(new { error = result.Error });
    
    return Ok();
}
```


#### Exceptions (Pouze Technical Errors)

**Exceptions používáme POUZE pro technical/infrastructure problémy:**

| Exception Type | Použití |
|----------------|---------|
| `DbUpdateException` | Database errors |
| `TimeoutException` | Network timeouts |
| `NullReferenceException` | Programming errors (bugs) |
| `InvalidOperationException` | Framework violations |

#### Result Pattern Guidelines

**✅ Kdy použít Result:**
- Vytváření entit (factory metody)
- Vytváření value objects
- Všechny business operace (Publish, Accept, Complete, Cancel...)
- Validace business pravidel
- Validace invariantů
- Jakákoliv operace, která může selhat z business důvodů

**❌ Kdy použít Exception:**
- Database connection errors
- Network failures
- File I/O errors
- Programming bugs (null refs, invalid cast)
- Framework violations

## 🏗️ Architektura

Projekt je založen na **Clean Architecture** a **Domain-Driven Design (DDD)**.

### Struktura Řešení

```
CraftsmenPlatform/
├── src/
│   ├── CraftsmenPlatform.Domain/              # Domain Layer - Core Business Logic
│   │   ├── Common/                            # Base classes (BaseEntity, IAggregateRoot, Result)
│   │   ├── Entities/                          # Domain Entities
│   │   ├── ValueObjects/                      # Domain Value Objects
│   │   ├── Enums/                             # Enumerations
│   │   ├── Events/                            # Domain Events (UserRegistered, etc.)
│   │   └── Repositories/                      # Repository Interfaces (IRepository, IUserRepository)
│   │
│   ├── CraftsmenPlatform.Application/         # Application Layer - Use Cases
│   │   ├── Commands/                          # CQRS Write Operations
│   │   ├── Queries/                           # CQRS Read Operations
│   │   ├── DTOs/                              # Data Transfer Objects
│   │   └── Common/                            # Behaviors, Interfaces
│   │
│   ├── CraftsmenPlatform.Infrastructure/      # Infrastructure Layer - External concerns
│   │   ├── Persistence/                       # EF Core DbContext, Configurations, Migrations
│   │   ├── Repositories/                      # Repository Implementations
│   │   ├── Events/                            # Domain Event Dispatchers
│   │   └── Services/                          # External Services Impl (Email, FileStorage)
│   │
│   └── CraftsmenPlatform.Api/                 # API Layer - Entry Point
│       ├── Controllers/                       # REST API Controllers
│       ├── Middleware/                        # Exception Handling, Logging
│       └── Extensions/                        # Service Registration Extensions
```

### Vrstvy

1. **Domain** - Obsahuje business logiku, agregáty, value objects, domain events
2. **Application** - CQRS pattern, MediatR handlers, DTOs
3. **Infrastructure** - Entity Framework, Repositories, External services
4. **API** - ASP.NET Core Web API, Controllers

## 🎯 Domain-Driven Design Implementation

### Agregáty (Aggregates)

Agregát je skupina souvisejících entit s transakční hranicí. Veškeré změny probíhají přes **Aggregate Root**.

#### ✅ Definované Agregáty

| Aggregate Root | Child Entities | Odpovědnost |
|---------------|----------------|-------------|
| `User` | - | Základní identita uživatele, email verifikace, profil |
| `Project` | `Offer`, `ProjectImage` | Správa projektů, nabídek, obrázků |
| `CraftsmanProfile` | `CraftsmanSkill` | Profil řemeslníka, dovednosti, rating |
| `CustomerProfile` | - | Profil zákazníka, statistiky |
| `Review` | - | Hodnocení řemeslníků |
| `ChatRoom` | `Message` | Chatovací místnost, zprávy |
| `Skill` | - | Reference data - dovednosti |
| `Category` | `CategorySkill` | Kategorie dovedností |

### Value Objects

Value objects jsou **immutable** a porovnávají se podle hodnoty, ne identity. Všechny mají **static factory metodu `Create`** vracející `Result<T>`.

| Value Object | Properties | Validace | Factory Metoda |
|--------------|-----------|----------|----------------|
| `EmailAddress` | `Value` | Email formát, max 255 chars | `Result<EmailAddress> Create(string)` |
| `Address` | `Street`, `City`, `State`, `ZipCode`, `Country` | Povinné pole | `Result<Address> Create(...)` |
| `PhoneNumber` | `Value` | Mezinárodní formát | `Result<PhoneNumber> Create(string)` |
| `Money` | `Amount`, `Currency` | Amount >= 0, Currency valid | `Result<Money> Create(decimal, string)` |
| `Rating` | `Value` (1-10) | Range 1-10 | `Result<Rating> Create(int)` |
| `DateRange` | `StartDate`, `EndDate` | StartDate <= EndDate | `Result<DateRange> Create(DateTime, DateTime)` |

**Příklad Value Object implementace:**
```csharp
public class EmailAddress : ValueObject
{
    public string Value { get; private set; }
    
    private EmailAddress(string value)
    {
        Value = value;
    }
    
    public static Result<EmailAddress> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<EmailAddress>.Failure("Email cannot be empty");
        
        if (value.Length > 255)
            return Result<EmailAddress>.Failure("Email cannot exceed 255 characters");
        
        if (!IsValidEmail(value))
            return Result<EmailAddress>.Failure("Invalid email format");
        
        return Result<EmailAddress>.Success(new EmailAddress(value));
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### Domain Events

Events reprezentují důležité business události v doméně.

| Event | Kdy nastane | Použití |
|-------|-------------|---------|
| `UserRegisteredEvent` | Nový uživatel se zaregistruje | Odeslání welcome emailu |
| `ProjectPublishedEvent` | Projekt je publikován | Notifikace řemeslníků |
| `OfferSubmittedEvent` | Řemeslník podá nabídku | Notifikace zákazníka |
| `OfferAcceptedEvent` | Zákazník akceptuje nabídku | Notifikace řemeslníka, zamítnutí ostatních |
| `ProjectCompletedEvent` | Projekt je dokončen | Aktualizace statistik |
| `ReviewPublishedEvent` | Hodnocení je publikováno | Aktualizace ratingu řemeslníka |

### Result Class Implementation

```csharp
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    
    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
    
    public static Result Success() => new Result(true, string.Empty);
    public static Result Failure(string error) => new Result(false, error);
}

public class Result<T> : Result
{
    public T Value { get; }
    
    private Result(bool isSuccess, T value, string error) 
        : base(isSuccess, error)
    {
        Value = value;
    }
    
    public static Result<T> Success(T value) => new Result<T>(true, value, string.Empty);
    public static Result<T> Failure(string error) => new Result<T>(false, default, error);
}
```

## 🔐 Authentication & Security

Systém používá **JWT (JSON Web Token)** based autentizaci s podporou refresh tokenů.

### Auth Flow
1. **Register**: Vytvoří uživatele, vytvoří hash hesla, vygeneruje tokens.
2. **Login**: Ověří email/heslo, vygeneruje Access + Refresh tokeny.
3. **RefreshToken**: Použije validní refresh token k získání nového access tokenu.
4. **Logout**: Revokuje refresh token (client-side remove, server-side flag).

### Komponenty

| Interface | Implementace (Infrastructure) | Účel |
|-----------|-------------------------------|------|
| `IJwtTokenGenerator` | `JwtTokenGenerator` | Generování Access a Refresh tokenů |
| `IPasswordHasher` | `PasswordHasher` | Hashing (BCrypt/PBKDF2) a verifikace hesel |
| `IRequestContext` | `HttpRequestContext` | Získání IP adresy, User ID z HttpContext |

### Token Strategy
- **Access Token**: Krátká platnost (např. 15 minut). Obsahuje Claims (Id, Email, Role).
- **Refresh Token**: Dlouhá platnost (např. 7 dní). Uložen v databázi (User Aggregate) s vazbou na zařízení/IP.

### Role-Based Authorization
V `Program.cs` jsou definovány policies:
- `RequireAdminRole`: Pouze Admin
- `RequireCraftsmanRole`: Craftsman nebo Admin
- `RequireCustomerRole`: Customer nebo Admin
- `RequireVerifiedEmail`: Uživatel musí mít ověřený email

### Příklad Implementace - Login Handler

```csharp
public async Task<Result<AuthenticationResponse>> Handle(LoginCommand request, CancellationToken ct)
{
    // 1. Validate credentials
    var user = await _userRepository.GetByEmailAsync(request.Email);
    if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        return Result.Failure("Invalid credentials");

    // 2. Generate tokens
    var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
    var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

    // 3. Store refresh token (Domain Logic) - může selhat
    var addTokenResult = user.AddRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7), ipAddress);
    if (addTokenResult.IsFailure)
        return Result<AuthenticationResponse>.Failure(addTokenResult.Error);
    
    // 4. Save & Return
    try
    {
        await _unitOfWork.SaveChangesAsync(ct);
    }
    catch (DbUpdateException ex)
    {
        return Result<AuthenticationResponse>.Failure($"Database error: {ex.Message}");
    }
    
    return Result<AuthenticationResponse>.Success(new AuthenticationResponse(accessToken, refreshToken));
}
```

## 📦 Klíčové Entity a jejich API

### User Aggregate

```csharp
// Factory Methods - vrací Result<User>
Result<User> User.CreateUser(email, passwordHash, firstName, lastName)
Result<User> User.CreateCraftsman(email, passwordHash, firstName, lastName)
Result<User> User.CreateCustomer(email, passwordHash, firstName, lastName)

// Domain Methods - všechny vrací Result
Result user.VerifyEmail()
Result user.UpdateProfile(firstName, lastName, phoneNumber, address, avatarUrl)
Result user.ChangePassword(newPasswordHash)
Result user.Deactivate(reason)
Result user.Activate()
Result user.RecordLogin()
Result user.ChangeRole(newRole)
Result user.AddRefreshToken(token, expiry, ipAddress) // Auth logic
```

**Business Rules:**
- Email musí být validní a unikátní (Result.Failure pokud ne)
- Nemůžeš se přihlásit s deaktivovaným účtem (Result.Failure)
- Verifikovaný email nelze znovu verifikovat (Result.Failure)

### Project Aggregate

```csharp
// Factory Method - vrací Result<Project>
Result<Project> Project.Create(customerId, title, description, budgetMin, budgetMax, ...)

// Domain Methods - všechny vrací Result nebo Result<T>
Result project.Publish()
Result<Offer> project.AddOffer(craftsmanId, price, description, ...)
Result project.AcceptOffer(offerId)
Result project.Complete()
Result project.Cancel(reason)
Result project.AddImage(imageUrl)
Result project.RemoveImage(imageId)
Result project.Update(title, description, ...)
```

**Business Rules:**
- Nabídky lze přidávat pouze k publikovaným projektům (Result.Failure)
- Projekt může mít max 1 akceptovanou nabídku (Result.Failure)
- Akceptování nabídky zamítne všechny ostatní pending nabídky
- Dokončit lze pouze projekt v InProgress stavu (Result.Failure)
- Update lze pouze v Draft stavu (Result.Failure)

### CraftsmanProfile Aggregate

```csharp
// Factory Method - vrací Result<CraftsmanProfile>
Result<CraftsmanProfile> CraftsmanProfile.Create(userId)

// Domain Methods - všechny vrací Result
Result profile.UpdateProfile(bio, registrationNumber, taxNumber, yearsOfExperience)
Result profile.Verify()
Result profile.Unverify()
Result profile.SetAvailability(isAvailable)
Result profile.AddSkill(skillId, yearsOfExperience, certificationLevel)
Result profile.RemoveSkill(skillId)

// Internal Methods (volané z event handlerů) - také vrací Result
Result profile.UpdateRating(Rating newRating)        // volá se při ReviewPublishedEvent
Result profile.IncrementCompletedProjects()          // volá se při ProjectCompletedEvent
```

**Business Rules:**
- Skill lze přidat pouze jednou (Result.Failure)
- Rating se aktualizuje automaticky při nové review
- Verifikovaný profil lze unverify

### Review Aggregate

```csharp
// Factory Method - vrací Result<Review>
Result<Review> Review.Create(userId, projectId, craftsmanId, ratingValue, comment)

// Domain Methods - všechny vrací Result
Result review.Publish()
Result review.Verify()
Result review.Update(newRating, newComment)
```

**Business Rules:**
- Rating musí být 1-10 (Result.Failure)
- Update lze pouze před publikací (Result.Failure)
- Publikované review nelze editovat (Result.Failure)
- Verifikovat lze pouze publikované review (Result.Failure)

### ChatRoom Aggregate

```csharp
// Factory Method - vrací Result<ChatRoom>
Result<ChatRoom> ChatRoom.Create(projectId, craftsmanId, customerId)

// Domain Methods - všechny vrací Result nebo Result<T>
Result<Message> chatRoom.SendMessage(senderId, content)
Result chatRoom.MarkMessagesAsRead(userId)
int chatRoom.GetUnreadCount(userId)  // Query metoda - nevrací Result
```

**Business Rules:**
- Craftsman může odesílat zprávy pouze customerovi a customer pouze craftsmanovi (Result.Failure)
- Max délka zprávy 5000 znaků (Result.Failure)

### Category Aggregate

```csharp
// Factory Method - vrací Result<Category>
Result<Category> Category.Create(name, description, iconUrl)

// Domain Methods - všechny vrací Result
Result category.Update(name, description, iconUrl)
Result category.Activate()
Result category.Deactivate()
Result category.AddSkill(skillId)
Result category.RemoveSkill(skillId)
```

**Business Rules:**
- Name musí být unikátní (Result.Failure v rámci kontextu, pokud je to vyžadováno)
- Name nesmí přesáhnout 100 znaků (Result.Failure)
- Nelze přidat duplicitní skill (Result.Failure)


## 🔧 Implementační Detaily

### Base Entities

```csharp
// BaseEntity - pro všechny entity
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public string CreatedBy { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public string? UpdatedBy { get; protected set; }
    public byte[] RowVersion { get; protected set; }
    
    // Domain Events
    public IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    protected void AddDomainEvent(IDomainEvent domainEvent)
    public void ClearDomainEvents()
}

// SoftDeletableEntity - pro soft delete
public abstract class SoftDeletableEntity : BaseEntity, ISoftDeletable
{
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    public string? DeletedBy { get; protected set; }
    
    public virtual void Delete(string deletedBy)
    public virtual void Restore()
}
```

### Encapsulation Pattern

**Všechny entity následují tento pattern:**

```csharp
public class MyEntity : BaseEntity, IAggregateRoot
{
    // 1. Properties - PRIVATE settery (encapsulation)
    public string Name { get; private set; }
    
    // 2. Private constructor pro EF Core
    private MyEntity() { }
    
    // 3. Private constructor pro business logiku
    private MyEntity(args) 
    {
        // Nastavení properties
        // Validace
    }
    
    // 4. Factory metoda (public static)
    public static MyEntity Create(args)
    {
        var entity = new MyEntity(args);
        entity.AddDomainEvent(new SomeEvent(...));
        return entity;
    }
    
    // 5. Domain methods (public)
    public void DoSomething(args)
    {
        // Business rules validace
        // Změna stavu
        // Domain events
    }
    
    // 6. Internal methods (pro komunikaci mezi agregáty)
    internal void InternalMethod(args)
    {
        // Volá se z jiných agregátů
    }
}
```

### Child Entities Pattern

```csharp
// Child entity - součást agregátu
public class ChildEntity : BaseEntity
{
    // 1. Foreign key na parent
    public Guid ParentId { get; private set; }
    
    // 2. Private constructor pro EF Core
    private ChildEntity() { }
    
    // 3. INTERNAL constructor - volá pouze parent aggregate
    internal ChildEntity(Guid parentId, args)
    {
        Id = Guid.NewGuid();
        ParentId = parentId;
        // ...
    }
    
    // 4. Internal methods - child lze měnit jen přes parent
    internal void SomeAction() { }
}
```

## 📋 Enums

### UserRole
```csharp
public enum UserRole { User, Craftsman, Admin }
```

### ProjectStatus
```csharp
public enum ProjectStatus 
{ 
    Draft,           // Koncept
    Published,       // Publikovaný - řemeslníci mohou nabízet
    InProgress,      // V realizaci
    Completed,       // Dokončeno
    Cancelled        // Zrušeno
}
```

### OfferStatus
```csharp
public enum OfferStatus 
{ 
    Pending,         // Čeká na odpověď
    Accepted,        // Akceptováno
    Rejected,        // Odmítnuto
    Withdrawn,       // Staženo řemeslníkem
    Expired          // Vypršelo
}
```

## 🔄 Komunikace Mezi Agregáty

### ❌ ŠPATNĚ - Direct reference
```csharp
// NIKDY toto nedělat!
var project = context.Projects.Include(p => p.Offers).First();
var offer = project.Offers.First();
offer.Accept(); // ❌ Porušuje aggregate boundary!
```

### ✅ SPRÁVNĚ - Přes Aggregate Root
```csharp
var project = context.Projects.Include(p => p.Offers).First();
var result = project.AcceptOffer(offerId); // ✅ Vše přes aggregate root
if (result.IsFailure)
{
    // Handle error
}
```

### Domain Events Pattern
```csharp
// 1. Aggregate vyhodí event
var result = review.Publish(); 
if (result.IsSuccess)
{
    // Vytvoří ReviewPublishedEvent
}

// 2. Event handler reaguje (NEVRACÍ Result, jen loguje chyby)
public class ReviewPublishedEventHandler : INotificationHandler<ReviewPublishedEvent>
{
    public async Task Handle(ReviewPublishedEvent @event, CancellationToken ct)
    {
        // Najdi CraftsmanProfile a aktualizuj rating
        var profile = await _context.CraftsmanProfiles
            .FirstAsync(p => p.Id == @event.CraftsmanId);
        
        var ratingResult = Rating.Create(@event.RatingValue);
        if (ratingResult.IsFailure)
        {
            _logger.LogError("Invalid rating value: {Error}", ratingResult.Error);
            return;
        }
        
        var updateResult = profile.UpdateRating(ratingResult.Value);
        if (updateResult.IsFailure)
        {
            _logger.LogError("Failed to update rating: {Error}", updateResult.Error);
            return;
        }
        
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save rating update");
        }
    }
}
```

## 🎨 Konvence a Best Practices

### Naming Conventions
- **Entities**: Pascal case, singular (User, Project, Offer)
- **Value Objects**: Pascal case, descriptive (EmailAddress, Money, Rating)
- **Events**: Pascal case, past tense + "Event" (UserRegisteredEvent)
- **Factory Methods**: "Create" - vždy vrací Result<T>
- **Domain Methods**: Slovesa (Publish, Accept, Update) - vrací Result nebo Result<T>

### Constructor Patterns
```csharp
// ✅ Doporučeno - private + factory s Result
private MyEntity(args) { ... }
public static Result<MyEntity> Create(args) 
{ 
    // Validace
    if (/* fail */) return Result<MyEntity>.Failure("Error");
    return Result<MyEntity>.Success(new MyEntity(args));
}

// ❌ Nedoporučeno - public constructor
public MyEntity(args) { ... }
```

### Property Setters
```csharp
// ✅ Domain entities - protected/private
public string Name { get; private set; }

// ✅ Child entities - private
public Guid ParentId { get; private set; }

// ✅ Value objects - NO setter (immutable)
public string Value { get; }
```

### Validace
```csharp
// ✅ SPRÁVNĚ - Pure Result Pattern
public static Result<Project> Create(string title, ...)
{
    if (string.IsNullOrWhiteSpace(title))
        return Result<Project>.Failure("Title cannot be empty");
    
    if (title.Length > 200)
        return Result<Project>.Failure("Title cannot exceed 200 characters");
    
    return Result<Project>.Success(new Project(title, ...));
}

// ✅ SPRÁVNĚ - Domain metoda s Result
public Result AcceptOffer(Guid offerId)
{
    if (Status != ProjectStatus.Published)
        return Result.Failure("Cannot accept offer for non-published project");
    
    var offer = _offers.FirstOrDefault(o => o.Id == offerId);
    if (offer == null)
        return Result.Failure("Offer not found");
    
    // Business logic
    return Result.Success();
}

// ❌ ŠPATNĚ - Nikdy nethroway exceptions v domain logice
public void AcceptOffer(Guid offerId)
{
    if (Status != ProjectStatus.Published)
        throw new BusinessRuleValidationException(...); // ❌ NO!
}
```

## 🗄️ Entity Framework Considerations

### Value Objects - Owned Types
```csharp
modelBuilder.Entity<User>()
    .OwnsOne(u => u.Email, email =>
    {
        email.Property(e => e.Value).HasColumnName("Email");
    });

modelBuilder.Entity<User>()
    .OwnsOne(u => u.Address, address =>
    {
        address.Property(a => a.Street).HasColumnName("AddressStreet");
        address.Property(a => a.City).HasColumnName("AddressCity");
        // ...
    });
```

### Collections - Backing Fields
```csharp
// V entity
private readonly List<Offer> _offers = new();
public IReadOnlyCollection<Offer> Offers => _offers.AsReadOnly();

// V EF configuration
modelBuilder.Entity<Project>()
    .HasMany(p => p.Offers)
    .WithOne()
    .HasForeignKey(o => o.ProjectId);
```

### Domain Events - Ignore
```csharp
modelBuilder.Entity<BaseEntity>()
    .Ignore(e => e.DomainEvents);
```

### Repository Pattern Interface

```csharp
public interface IRepository<T> where T : BaseEntity, IAggregateRoot
{
    // Queries
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    
    // Commands
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
```

### Unit of Work Pattern API

```csharp
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

## 🚀 Další Kroky

### TODO - Infrastructure
- [x] EF Core DbContext konfigurace pro všechny entity
- [x] Value Objects jako Owned Types
- [x] Repository pattern
- [x] Unit of Work pattern
- [x] Domain Events dispatcher

### TODO - Application
- [/] CQRS Commands a Queries
- [/] MediatR Handlers
- [ ] FluentValidation validators
- [/] DTOs a Mapping

### TODO - API
- [/] Controllers
- [/] Authentication & Authorization
- [ ] API Documentation (Swagger)

## 📝 Poznámky pro AI Agenty

### Při práci s tímto projektem:

1. **Vždy respektuj aggregate boundaries** - Změny pouze přes aggregate root
2. **Používej factory metody** - Ne public constructory
3. **Value Objects jsou immutable** - Nelze měnit po vytvoření
4. **Validace patří do domény** - Ne do aplikační vrstvy
5. **Domain events pro komunikaci** - Ne direct references mezi agregáty
6. **Protected settery v entitách** - Encapsulation je klíčová
7. **Child entities s internal constructory** - Pouze parent je může vytvořit

### Při přidávání nové funkcionality:

1. Identifikuj který agregát je odpovědný
2. Přidej domain metodu do aggregate root
3. Validuj business rules
4. Přidej domain event pokud potřeba
5. Vytvoř handler pro event
6. Přidej CQRS command/query v Application layer
7. Přidej endpoint v API layer

---

**Poslední aktualizace**: 2025-12-25
**DDD Refactoring**: ✅ Kompletní
**Result Pattern**: ✅ Implementováno ve všech agregátech
**Status projektu**: Domain a Infrastructure vrstvy hotové. Application a API vrstvy rozpracovány (Authentication).
