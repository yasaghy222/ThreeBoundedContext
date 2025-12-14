# Logging

Shared extension for **consistent logging and correlation IDs** across all microservices using **Serilog** and **Seq**.

## Purpose

This extension provides:

- Centralized Serilog configuration with Seq sink
- Correlation ID generation and propagation
- Request/response logging with consistent format
- Scope-based logging for traceability
- Machine and thread enrichment

## Usage

### 1. Add package reference

```xml
<ItemGroup>
  <ProjectReference Include="../../Shared/Shared.Logging/Logging.csproj" />
</ItemGroup>
```

### 2. Register in Program.cs

```csharp
using Logging;

// Configure logging with service name
builder.AddLoggingExtension("YourServiceName");

// ... after app.Build()

app.UseLoggingExtension(); // Adds Serilog request logging + correlation ID middleware

// Recommended: wrap app.Run() in try-catch for proper log flushing
try
{
    Log.Information("Starting YourServiceName...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "YourServiceName terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

### 3. Configure appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://seq:80"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"],
    "Properties": {
      "Application": "YourServiceName"
    }
  }
}
```

### 4. Use ILogger in your code

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

## Seq Dashboard

Access the Seq dashboard at `http://localhost:5341` to view and search logs from all services.

### Docker Compose

The Seq service is configured in `src/Deploy/Seq/docker-compose.yml` and included in the root `docker-compose.yml`.

```yaml
services:
  seq:
    image: datalust/seq:latest
    container_name: seq
    ports:
      - "5341:80" # Web UI
      - "5342:5341" # Ingestion API
    volumes:
      - ./data:/data
```

## Correlation ID

Every request gets a unique correlation ID that propagates through logs:

```
[10:30:45 INF] [abc123] Fetching customer 12345
[10:30:45 INF] [abc123] Customer found
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

| Level         | When to Use                                    |
| ------------- | ---------------------------------------------- |
| `Trace`       | Detailed debugging (disabled in production)    |
| `Debug`       | Development debugging                          |
| `Information` | Normal operations (request started, completed) |
| `Warning`     | Recoverable issues (retry, fallback)           |
| `Error`       | Failures that need attention                   |
| `Critical`    | System-wide failures                           |

## Environment Variables

You can override the Seq URL using environment variables:

```bash
SEQ_URL=http://seq:80
```

## Best Practices

- Use structured logging: `_logger.LogInformation("User {UserId} logged in", userId)`
- **Never** log sensitive data (passwords, tokens, PII)
- Include correlation IDs in external service calls
- Use appropriate log levels
- Use Seq dashboard to search and filter logs across all services

---

# Logging (فارسی)

افزونه مشترک برای **لاگ‌گذاری یکسان و شناسه همبستگی** در تمام میکروسرویس‌ها با استفاده از **Serilog** و **Seq**.

## هدف

این افزونه امکانات زیر را فراهم می‌کند:

- پیکربندی مرکزی Serilog با sink به Seq
- تولید و انتشار شناسه همبستگی (Correlation ID)
- لاگ درخواست/پاسخ با فرمت یکسان
- لاگ‌گذاری مبتنی بر Scope برای ردیابی

## نحوه استفاده

```csharp
using Logging;

builder.AddLoggingExtension("YourServiceName");
app.UseLoggingExtension();
```

## داشبورد Seq

به داشبورد Seq در `http://localhost:5341` دسترسی پیدا کنید تا لاگ‌های تمام سرویس‌ها را مشاهده و جستجو کنید.

## شناسه همبستگی

هر درخواست یک شناسه همبستگی منحصر به فرد دریافت می‌کند که در تمام لاگ‌ها منتشر می‌شود و امکان ردیابی یک درخواست در سراسر سیستم را فراهم می‌کند.

## بهترین شیوه‌ها

- از لاگ‌گذاری ساختاریافته استفاده کنید
- **هرگز** داده‌های حساس (رمز عبور، توکن، PII) را لاگ نکنید
- شناسه همبستگی را در فراخوانی سرویس‌های خارجی شامل کنید
- از داشبورد Seq برای جستجو و فیلتر لاگ‌ها در تمام سرویس‌ها استفاده کنید
