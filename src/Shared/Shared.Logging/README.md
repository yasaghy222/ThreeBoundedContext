# Logging

Shared extension for **consistent logging and correlation IDs** across all microservices.

## Purpose

This extension provides:
- Centralized logging configuration
- Correlation ID generation and propagation
- Request/response logging with consistent format
- Scope-based logging for traceability

## Usage

### 1. Add package reference

```xml
<ItemGroup>
  <ProjectReference Include="../../shared/Logging/Logging.csproj" />
</ItemGroup>
```

### 2. Register in Program.cs

```csharp
using Logging;

builder.AddLoggingExtension();

// ... after app.Build()

app.UseLoggingExtension(); // Adds correlation ID middleware
```

### 3. Use ILogger in your code

```csharp
public class CustomerService
{
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(ILogger<CustomerService> logger)
    {
        _logger = logger;
    }

    public async Task<Customer> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Fetching customer {CustomerId}", id);
        // ... implementation
    }
}
```

## Correlation ID

Every request gets a unique correlation ID that propagates through logs:

```
[2024-01-15 10:30:45] [Information] [CorrelationId: abc-123] Fetching customer 12345
[2024-01-15 10:30:45] [Information] [CorrelationId: abc-123] Customer found
```

### Accessing Correlation ID

```csharp
using Logging.Abstractions;

public class MyService
{
    private readonly ICorrelationIdAccessor _correlationAccessor;

    public MyService(ICorrelationIdAccessor correlationAccessor)
    {
        _correlationAccessor = correlationAccessor;
    }

    public void DoWork()
    {
        var correlationId = _correlationAccessor.CorrelationId;
        // Use for external API calls, message headers, etc.
    }
}
```

### Propagating to External Services

```csharp
httpClient.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);
```

## Log Levels

| Level | When to Use |
|-------|-------------|
| `Trace` | Detailed debugging (disabled in production) |
| `Debug` | Development debugging |
| `Information` | Normal operations (request started, completed) |
| `Warning` | Recoverable issues (retry, fallback) |
| `Error` | Failures that need attention |
| `Critical` | System-wide failures |

## Best Practices

- Use structured logging: `_logger.LogInformation("User {UserId} logged in", userId)`
- **Never** log sensitive data (passwords, tokens, PII)
- Include correlation IDs in external service calls
- Use appropriate log levels

---

# Logging (فارسی)

افزونه مشترک برای **لاگ‌گذاری یکسان و شناسه همبستگی** در تمام میکروسرویس‌ها.

## هدف

این افزونه امکانات زیر را فراهم می‌کند:
- پیکربندی مرکزی لاگ‌گذاری
- تولید و انتشار شناسه همبستگی (Correlation ID)
- لاگ درخواست/پاسخ با فرمت یکسان
- لاگ‌گذاری مبتنی بر Scope برای ردیابی

## نحوه استفاده

```csharp
using Logging;

builder.AddLoggingExtension();
app.UseLoggingExtension();
```

## شناسه همبستگی

هر درخواست یک شناسه همبستگی منحصر به فرد دریافت می‌کند که در تمام لاگ‌ها منتشر می‌شود و امکان ردیابی یک درخواست در سراسر سیستم را فراهم می‌کند.

## بهترین شیوه‌ها

- از لاگ‌گذاری ساختاریافته استفاده کنید
- **هرگز** داده‌های حساس (رمز عبور، توکن، PII) را لاگ نکنید
- شناسه همبستگی را در فراخوانی سرویس‌های خارجی شامل کنید
