# Rate Limiting Documentation

## 🚦 Rate Limiting Strategy

CraftsmenPlatform používá **multi-layer rate limiting strategii** pro ochranu API před zneužitím a zajištění fair usage mezi uživateli.

### Proč Rate Limiting?

| Důvod | Popis |
|-------|-------|
| **DoS/DDoS Protection** | Ochrana proti denial-of-service útokům |
| **Brute-force Prevention** | Ochrana login/auth endpointů |
| **Fair Resource Usage** | Zajištění spravedlivého přístupu ke zdrojům |
| **Server Stability** | Prevence přetížení serveru |
| **Cost Control** | Kontrola nákladů na cloud infrastrukturu |

### Technology

- **Built-in ASP.NET Core Rate Limiting** (od .NET 7)
- Namespace: `Microsoft.AspNetCore.RateLimiting`
- Zero external dependencies
- Production-ready a optimalizované

---

## 🎯 Rate Limiting Policies

### 1. Global IP-based Protection (`global`)

**Algorithm**: Fixed Window Limiter  
**Limit**: 100 requests/minute per IP  
**Queue**: 10 requests  

```csharp
.AddFixedWindowLimiter("global", opt =>
{
    opt.PermitLimit = 100;
    opt.Window = TimeSpan.FromMinutes(1);
    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    opt.QueueLimit = 10;
});
```

**Použití**: Automaticky aplikováno na všechny endpointy jako základní ochrana.

**Business Rules**:
- IP adresa může poslat max 100 requestů za minutu
- Dalších 10 requestů může čekat ve frontě
- Po překročení limitu → HTTP 429 (Too Many Requests)

---

### 2. Authentication Endpoints (`auth`)

**Algorithm**: Sliding Window Limiter  
**Limit**: 5 requests/minute per IP  
**Queue**: 2 requests  
**Segments**: 2 per window  

```csharp
.AddSlidingWindowLimiter("auth", opt =>
{
    opt.PermitLimit = 5;
    opt.Window = TimeSpan.FromMinutes(1);
    opt.SegmentsPerWindow = 2;
    opt.QueueLimit = 2;
});
```

**Použití**: Login, Register, Password Reset endpointy

**Business Rules**:
- Velmi restriktivní limit pro ochranu před brute-force
- Sliding window zajišťuje plynulejší reset (každých 30s přidá polovinu limitu)
- Max 5 pokusů o login za minutu

**Aplikace**:
```csharp
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login() { }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register() { }
}
```

---

### 3. Per-User Rate Limiting (`per-user`)

**Algorithm**: Token Bucket Limiter  
**Limit**: 30 requests/minute per authenticated user  
**Replenishment**: 30 tokens/minute (auto-replenish)  

```csharp
.AddPolicy("per-user", context =>
{
    var userId = context.User?.FindFirst("sub")?.Value ?? "anonymous";
    
    return RateLimitPartition.GetTokenBucketLimiter(userId, _ => 
        new TokenBucketRateLimiterOptions
        {
            TokenLimit = 30,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = 30,
            AutoReplenishment = true
        });
});
```

**Použití**: Většina authenticated endpointů (Projects, Offers, Reviews, Chat)

**Business Rules**:
- Každý uživatel má vlastní "bucket" s 30 tokeny
- Každý request spotřebuje 1 token
- Tokeny se automaticky doplňují (30 za minutu)
- Umožňuje burst traffic (všech 30 requestů najednou je OK)

**Aplikace**:
```csharp
[Authorize]
[EnableRateLimiting("per-user")]
public class ProjectsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateProject() { }
    
    [HttpGet]
    [DisableRateLimiting] // Public read - bez limitu
    public async Task<IActionResult> GetProjects() { }
}
```

---

### 4. Resource-Intensive Operations (`concurrent`)

**Algorithm**: Concurrency Limiter  
**Limit**: 3 concurrent requests per user  
**Queue**: 5 requests  

```csharp
.AddConcurrencyLimiter("concurrent", opt =>
{
    opt.PermitLimit = 3;
    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    opt.QueueLimit = 5;
});
```

**Použití**: Image uploads, File processing, AI operations

**Business Rules**:
- Maximálně 3 současné requesty na daný endpoint
- Další requesty čekají ve frontě (max 5)
- Ideální pro operace s vysokým memory/CPU footprintem

**Aplikace**:
```csharp
[Authorize]
[EnableRateLimiting("concurrent")]
public class ImagesController : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage() { }
}
```

---

## 🔧 Implementation Details

### Program.cs Setup

```csharp
// 1. Register services
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    // ... add all policies (viz výše)
    
    // Custom rejection handler
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = 
                retryAfter.TotalSeconds.ToString();
        }
        
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests",
            message = "Rate limit exceeded. Please try again later.",
            retryAfter = retryAfter?.TotalSeconds
        }, cancellationToken);
    };
});

// 2. Add middleware (before UseAuthorization)
app.UseRateLimiter();
app.UseAuthorization();
```

### Response Format při 429

```json
{
  "error": "Too many requests",
  "message": "Rate limit exceeded. Please try again later.",
  "retryAfter": 42.5
}
```

### Response Headers

Automaticky přidané headers:
```
X-RateLimit-Limit: 30
X-RateLimit-Remaining: 15
X-RateLimit-Reset: 1703001234
Retry-After: 42
```

---

## 📊 Rate Limiting Algorithms

| Algorithm | Use Case | Pros | Cons |
|-----------|----------|------|------|
| **Fixed Window** | Global protection | Jednoduchý, předvídatelný | Burst na hranici window |
| **Sliding Window** | Auth endpoints | Plynulejší, lépe řeší burst | Složitější implementace |
| **Token Bucket** | Per-user limits | Umožňuje burst, fair | Složité nastavení |
| **Concurrency** | Resource-heavy ops | Chrání resources přímo | Nevhodné pro quick requests |

### Kdy použít který?

```
┌─────────────────────────────────────────────────────────────┐
│ Request Type          │ Algorithm        │ Policy           │
├───────────────────────┼──────────────────┼──────────────────┤
│ Public API reads      │ Fixed Window     │ global           │
│ Login/Register        │ Sliding Window   │ auth             │
│ User CRUD operations  │ Token Bucket     │ per-user         │
│ File uploads          │ Concurrency      │ concurrent       │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎨 Controller Usage Patterns

### Pattern 1: Controller-level Policy

```csharp
[EnableRateLimiting("per-user")]
public class ProjectsController : ControllerBase
{
    // Všechny actions mají "per-user" limit
}
```

### Pattern 2: Action-level Override

```csharp
[EnableRateLimiting("per-user")]
public class ProjectsController : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("concurrent")] // Override controller policy
    public async Task<IActionResult> CreateProject() { }
    
    [HttpGet]
    [DisableRateLimiting] // Disable rate limiting
    public async Task<IActionResult> GetPublicProjects() { }
}
```

### Pattern 3: Multiple Policies (Chaining)

```csharp
// Používáme kombinaci "global" + "per-user"
// Global je automaticky, per-user přidáme explicitně
[Authorize]
[EnableRateLimiting("per-user")]
public class OffersController : ControllerBase
{
    // User může poslat 30 req/min
    // + IP může poslat max 100 req/min (global)
}
```

---

## 🚀 Production Considerations

### 1. Distributed Cache (Redis)

Pro **multi-instance deployment** (load balanced servers) je potřeba sdílený cache:

```csharp
// TODO: Implementovat pro production
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "CraftsmenPlatform:RateLimit:";
});
```

**Důležité**: Built-in rate limiter používá **in-memory storage**, což funguje pouze pro **single-instance deployment**.

### 2. Monitoring & Alerts

```csharp
// OpenTelemetry metrics
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddMeter("Microsoft.AspNetCore.RateLimiting");
    });
```

**Metrics to monitor**:
- `rate_limit_requests_rejected` - Počet zamítnutých requestů
- `rate_limit_lease_duration` - Jak dlouho request čekal
- `rate_limit_queued_requests` - Aktuální fronta

### 3. Configuration per Environment

```json
// appsettings.Production.json
{
  "RateLimiting": {
    "Global": {
      "PermitLimit": 100,
      "WindowMinutes": 1
    },
    "Auth": {
      "PermitLimit": 5,
      "WindowMinutes": 1
    },
    "PerUser": {
      "TokenLimit": 30,
      "ReplenishmentMinutes": 1
    }
  }
}
```

### 4. Graceful Degradation

```csharp
options.OnRejected = async (context, cancellationToken) =>
{
    // Log pro security monitoring
    _logger.LogWarning(
        "Rate limit exceeded for {Path} from {IP}",
        context.HttpContext.Request.Path,
        context.HttpContext.Connection.RemoteIpAddress
    );
    
    // Custom response
    await context.HttpContext.Response.WriteAsJsonAsync(new
    {
        error = "Too many requests",
        message = "Please slow down your requests.",
        retryAfter = retryAfter?.TotalSeconds
    }, cancellationToken);
};
```

---

## 🧪 Testing Strategy

### Integration Tests

```csharp
public class RateLimitingTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task AuthLogin_ExceedsLimit_Returns429()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new { email = "test@test.com", password = "pass" };
        
        // Act - Fire 6 requests (limit is 5)
        var tasks = Enumerable.Range(0, 6)
            .Select(_ => client.PostAsJsonAsync("/api/auth/login", loginRequest));
        
        var responses = await Task.WhenAll(tasks);
        
        // Assert
        var rejectedCount = responses.Count(r => 
            r.StatusCode == HttpStatusCode.TooManyRequests);
        
        Assert.True(rejectedCount >= 1);
    }
    
    [Fact]
    public async Task RejectedRequest_IncludesRetryAfterHeader()
    {
        // ... similar test
        
        var rejectedResponse = responses.First(r => 
            r.StatusCode == HttpStatusCode.TooManyRequests);
        
        Assert.True(rejectedResponse.Headers.Contains("Retry-After"));
    }
}
```

### Load Testing

```bash
# Apache Bench
ab -n 1000 -c 10 http://localhost:5000/api/projects

# K6
k6 run --vus 10 --duration 30s rate-limit-test.js
```

---

## 📋 Checklist pro Production

- [ ] Rate limiting policies definovány pro všechny kritické endpointy
- [ ] Auth endpointy mají strict limits (5-10 req/min)
- [ ] Public read endpointy mají generous limits nebo disable
- [ ] Custom OnRejected handler s user-friendly message
- [ ] Redis cache pro distributed deployment
- [ ] Monitoring & alerting na rate limit violations
- [ ] Load testing provedeno s expected traffic
- [ ] Documentation pro API consumers (v Swagger)
- [ ] Security team review

---

## 🔄 Future Improvements

### Phase 1 (Implemented)
- [x] Basic rate limiting policies
- [x] Per-user rate limiting
- [x] Custom rejection responses

### Phase 2 (Planned)
- [ ] Redis distributed cache
- [ ] Dynamic rate limits based on user subscription tier
- [ ] Rate limit exemptions for trusted clients
- [ ] Advanced metrics & dashboards

### Phase 3 (Future)
- [ ] AI-based anomaly detection
- [ ] Geographic-based rate limiting
- [ ] Adaptive rate limiting based on server load

---

**Best Practices**:
1. **Start conservative, relax later** - Začněte s nižšími limity a zvyšujte podle potřeby
2. **Monitor continuously** - Sledujte rejections a adjustujte
3. **Document for clients** - API consumers musí vědět o limitech
4. **Test under load** - Vždy load test před production deployment
5. **Plan for scaling** - Redis pro multi-instance deployment

**Security Notes**:
- Rate limiting **NENÍ** kompletní DDoS protection
- Pro produkci doporučujeme cloudflare/AWS Shield
- Kombinujte s WAF (Web Application Firewall)
- IP-based limiting lze obejít (proxies/VPN) - proto per-user limity