# ErrorHandling

Shared extension for **consistent error responses** across all microservices using ProblemDetails format.

## Purpose

This extension provides:
- Centralized exception handling middleware
- Consistent ProblemDetails JSON responses (RFC 7807)
- Custom exception types for different error scenarios
- Validation error formatting
- No sensitive data leakage in error responses

## Usage

### 1. Add package reference

```xml
<ItemGroup>
  <ProjectReference Include="../../shared/ErrorHandling/ErrorHandling.csproj" />
</ItemGroup>
```

### 2. Register in Program.cs

```csharp
using ErrorHandling;

builder.Services.AddErrorHandling();

// ... after app.Build()

app.UseErrorHandling(); // Must be early in pipeline
```

### 3. Throw exceptions in your code

```csharp
using ErrorHandling.Core.Exceptions;

// 404 Not Found
throw new NotFoundException("Customer not found.");

// 400 Bad Request
throw new BadRequestException("Invalid phone number format.");

// 401 Unauthorized
throw new UnauthorizedException("Token expired.");

// 502 Bad Gateway (external service failure)
throw new ExternalServiceException("Sepidar API timeout.");

// 501 Not Implemented
throw new NotImplementedFeatureException("Payment refund not yet supported.");
```

## Exception Types

| Exception | HTTP Status | When to Use |
|-----------|-------------|-------------|
| `NotFoundException` | 404 | Resource not found |
| `BadRequestException` | 400 | Invalid input/validation failure |
| `UnauthorizedException` | 401 | Authentication failed |
| `ForbiddenException` | 403 | Access denied |
| `ConflictException` | 409 | Resource conflict |
| `ExternalServiceException` | 502 | External API failure |
| `NotImplementedFeatureException` | 501 | Feature not yet implemented |

## Response Format

All errors return ProblemDetails JSON:

```json
{
  "type": "https://httpstatuses.io/404",
  "title": "Not Found",
  "status": 404,
  "detail": "Customer not found.",
  "instance": "/v1/api/Customer/12345",
  "errorCode": "not_found",
  "traceId": "0HMVQ..."
}
```

### Validation Errors

```json
{
  "type": "https://httpstatuses.io/400",
  "title": "Validation failed.",
  "status": 400,
  "detail": "Refer to the errors collection for additional details.",
  "errors": {
    "PhoneNumber": ["Phone number is required.", "Invalid format."]
  },
  "errorCode": "validation_error",
  "traceId": "0HMVQ..."
}
```

## Security Notes

- Exception messages are included in responses - avoid sensitive data
- Stack traces are **never** included in responses
- Detailed errors are logged server-side only

---

# ErrorHandling (فارسی)

افزونه مشترک برای **پاسخ‌های خطای یکسان** در تمام میکروسرویس‌ها با فرمت ProblemDetails.

## هدف

این افزونه امکانات زیر را فراهم می‌کند:
- Middleware مرکزی برای مدیریت استثناها
- پاسخ‌های یکسان ProblemDetails JSON (RFC 7807)
- انواع استثنای سفارشی برای سناریوهای مختلف
- فرمت‌دهی خطاهای اعتبارسنجی
- عدم نشت اطلاعات حساس در پاسخ‌های خطا

## نحوه استفاده

```csharp
using ErrorHandling;
using ErrorHandling.Core.Exceptions;

builder.Services.AddErrorHandling();
app.UseErrorHandling();

// در کد:
throw new NotFoundException("مشتری یافت نشد.");
throw new BadRequestException("فرمت شماره تلفن نامعتبر است.");
```

## نکات امنیتی

- پیام‌های استثنا در پاسخ‌ها شامل می‌شوند - از داده‌های حساس اجتناب کنید
- Stack trace **هرگز** در پاسخ‌ها شامل نمی‌شود
- خطاهای جزئی فقط در لاگ سرور ثبت می‌شوند
