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

Projekt používá **hybrid approach** pro error handling:

#### Result Pattern (Business Validace)
Pro **business rule validace** v domain metodách používáme **Result pattern** místo exceptions:

```csharp
public Result AcceptOffer(Guid offerId)
{
    if (Status != ProjectStatus.Published)
        return Result.Failure("Cannot accept offer for non-published project");
    
    // ... business logic
    return Result.Success();
}

// S return value
public Result<Offer> AddOffer(...)
{
    if (Status != ProjectStatus.Published)
        return Result<Offer>.Failure("Cannot add offer to non-published project");
    
    var offer = new Offer(...);
    return Result<Offer>.Success(offer);
}
```

**Použití:**
```csharp
var result = project.AcceptOffer(offerId);
if (result.IsFailure)
{
    // Handle error - např. vrátit BadRequest s result.Error
    return BadRequest(result.Error);
}
// Success path
```

#### Exceptions (Technical Validace)
Pro **technical validace** (nevalidní data, porušení invariantů) používáme **exceptions**:

```csharp
// Value Objects - vždy throwují při invalid input
var email = EmailAddress.Create("invalid");  // throws InvalidValueObjectException

// Constructory - validace invariantů
private Project(args)
{
    if (string.IsNullOrWhiteSpace(title))
        throw new BusinessRuleValidationException(nameof(Title), "Title cannot be empty");
}
```

**Kdy co použít:**
- ✅ **Result** - Business operace (Publish, AcceptOffer, Complete, Cancel...)
- ✅ **Exception** - Value Object validace, Constructor validace, Technical errors

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
│   │   ├── Exceptions/                        # Domain Exceptions
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

Value objects jsou **immutable** a porovnávají se podle hodnoty, ne identity.

| Value Object | Properties | Validace |
|--------------|-----------|----------|
| `EmailAddress` | `Value` | Email formát, max 255 chars |
| `Address` | `Street`, `City`, `State`, `ZipCode`, `Country` | Povinné pole |
| `PhoneNumber` | `Value` | Mezinárodní formát |
| `Money` | `Amount`, `Currency` | Amount >= 0, Currency valid |
| `Rating` | `Value` (1-10) | Range 1-10 |
| `DateRange` | `StartDate`, `EndDate` | StartDate <= EndDate |

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

### Domain Exceptions

| Exception | Použití |
|-----------|---------|
| `DomainException` | Base exception pro všechny domain exceptions |
| `BusinessRuleValidationException` | Porušení business pravidel |
| `InvalidValueObjectException` | Nevalidní value object |

## 📦 Klíčové Entity a jejich API

### User Aggregate

```csharp
// Factory Methods
User.CreateUser(email, passwordHash, firstName, lastName)
User.CreateCraftsman(email, passwordHash, firstName, lastName)
User.CreateCustomer(email, passwordHash, firstName, lastName)

// Domain Methods
user.VerifyEmail()
user.UpdateProfile(firstName, lastName, phoneNumber, address, avatarUrl)
user.ChangePassword(newPasswordHash)
user.Deactivate(reason)
user.Activate()
user.RecordLogin()
user.ChangeRole(newRole)
```

**Business Rules:**
- Email musí být validní a unikátní
- Nemůžeš se přihlásit s deaktivovaným účtem
- Verifikovaný email nelze znovu verifikovat

### Project Aggregate

```csharp
// Factory Method
Project.Create(customerId, title, description, budgetMin, budgetMax, ...)

// Domain Methods
project.Publish()
project.AddOffer(craftsmanId, price, description, ...)
project.AcceptOffer(offerId)
project.Complete()
project.Cancel(reason)
project.AddImage(imageUrl)
project.RemoveImage(imageId)
project.Update(title, description, ...)
```

**Business Rules:**
- Nabídky lze přidávat pouze k publikovaným projektům
- Projekt může mít max 1 akceptovanou nabídku
- Akceptování nabídky zamítne všechny ostatní pending nabídky
- Dokončit lze pouze projekt v InProgress stavu
- Update lze pouze v Draft stavu

### CraftsmanProfile Aggregate

```csharp
// Factory Method
CraftsmanProfile.Create(userId)

// Domain Methods
profile.UpdateProfile(bio, registrationNumber, taxNumber, yearsOfExperience)
profile.Verify()
profile.Unverify()
profile.SetAvailability(isAvailable)
profile.AddSkill(skillId, yearsOfExperience, certificationLevel)
profile.RemoveSkill(skillId)

// Internal Methods (volané z jiných agregátů)
profile.UpdateRating(newRating)        // volá se při ReviewPublishedEvent
profile.IncrementCompletedProjects()   // volá se při ProjectCompletedEvent
```

**Business Rules:**
- Skill lze přidat pouze jednou
- Rating se aktualizuje automaticky při nové review
- Verifikovaný profil lze unverify

### Review Aggregate

```csharp
// Factory Method
Review.Create(userId, projectId, craftsmanId, ratingValue, comment)

// Domain Methods
review.Publish()
review.Verify()
review.Update(newRating, newComment)
```

**Business Rules:**
- Rating musí být 1-10
- Update lze pouze před publikací
- Publikované review nelze editovat
- Verifikovat lze pouze publikované review

### ChatRoom Aggregate

```csharp
// Factory Method
ChatRoom.Create(projectId, craftsmanId, customerId)

// Domain Methods
chatRoom.SendMessage(senderId, content)
chatRoom.MarkMessagesAsRead(userId)
chatRoom.GetUnreadCount(userId)
```

**Business Rules:**
- Zprávu může odeslat pouze craftsman nebo customer
- Max délka zprávy 5000 znaků

### Category Aggregate

```csharp
// Factory Method
Category.Create(name, description, iconUrl)

// Domain Methods
category.Update(name, description, iconUrl)
category.Activate()
category.Deactivate()
category.AddSkill(skillId)
category.RemoveSkill(skillId)
```

**Business Rules:**
- Name musí být unikátní (v rámci kontextu, pokud je to vyžadováno)
- Name nesmí přesáhnout 100 znaků
- Nelze přidat duplicitní skill


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
project.AcceptOffer(offerId); // ✅ Vše přes aggregate root
```

### Domain Events Pattern
```csharp
// 1. Aggregate vyhodí event
review.Publish(); // Vytvoří ReviewPublishedEvent

// 2. Event handler reaguje
public class ReviewPublishedEventHandler : INotificationHandler<ReviewPublishedEvent>
{
    public async Task Handle(ReviewPublishedEvent @event)
    {
        // Najdi CraftsmanProfile a aktualizuj rating
        var profile = await _context.CraftsmanProfiles
            .FirstAsync(p => p.Id == @event.CraftsmanId);
            
        var rating = Rating.Create(@event.Rating);
        profile.UpdateRating(rating);
    }
}
```

## 🎨 Konvence a Best Practices

### Naming Conventions
- **Entities**: Pascal case, singular (User, Project, Offer)
- **Value Objects**: Pascal case, descriptive (EmailAddress, Money, Rating)
- **Events**: Pascal case, past tense + "Event" (UserRegisteredEvent)
- **Exceptions**: Pascal case + "Exception" (BusinessRuleValidationException)

### Constructor Patterns
```csharp
// ✅ Doporučeno - private + factory
private MyEntity(args) { ... }
public static MyEntity Create(args) { ... }

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
// V konstruktoru nebo factory metodě
if (string.IsNullOrWhiteSpace(title))
    throw new BusinessRuleValidationException(nameof(Title), "Title cannot be empty");

// V domain methodách
if (Status != ProjectStatus.Published)
    throw new BusinessRuleValidationException(
        nameof(AcceptOffer), 
        "Cannot accept offer for non-published project");
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
- [ ] CQRS Commands a Queries
- [ ] MediatR Handlers
- [ ] FluentValidation validators
- [ ] DTOs a Mapping

### TODO - API
- [ ] Controllers
- [ ] Authentication & Authorization
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

**Poslední aktualizace**: 2025-12-22
**DDD Refactoring**: ✅ Kompletní
**Result Pattern**: ✅ Implementováno ve všech agregátech
**Status projektu**: Domain vrstva hotová, Infrastructure layer (EF Core configs, events, Repositories, UnitOfWork) implementována.

