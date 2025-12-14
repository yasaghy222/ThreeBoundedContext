# Swagger Extension

اکستنشن Swagger این مخزن برای تمام سرویس‌ها رفتار یکسانی فراهم می‌کند.

## قابلیت‌ها

- ثبت داکیومنت‌های متنوع بر اساس تنظیمات `Swagger` در appsettings.
- فعال‌سازی Bearer Authentication در صورت نیاز.
- اعمال `DefaultValuesOperationFilter` برای قرار دادن نمونه مقدار روی پارامترهای اختیاری.
- **نمایش توضیح کامل enum**: برای هر اسکیمایی که شامل enum باشد، نام و مقدار عددی هر عضو به‌صورت `Name = Value` به توضیحات اسکیمای مربوط افزوده می‌شود و علاوه بر آن در افزونه‌های `x-enumNames` و `x-enumDescriptions` قرار می‌گیرد؛ بنابراین Swagger UI نام و مقدار هر enum (مانند `RealCustomer = 1`) را نمایش می‌دهد.
- **نام ساده برای enumها**: شناسه‌ی اسکیمای هر enum فقط نام خود enum است (بدون namespace) تا مستندات خواناتر شود.
- **جمع شدن پیش‌فرض گروه‌ها**: در Swagger UI تمام دسته‌بندی‌ها (tags) به‌صورت پیش‌فرض بسته هستند و کاربر می‌تواند آن‌ها را به‌صورت دستی باز کند.
- پنهان بودن مدل‌ها در نمای اصلی (`DefaultModelsExpandDepth = -1`).
- فعال بودن فیلتر جست‌وجو و نمایش مدت زمان اجرای درخواست (قابل تنظیم از طریق `Swagger:Ui`).

### ترتیب نمایش اندپوینت‌ها

برای خوانایی بهتر، ترتیب نمایش عملیات‌ها داخل هر گروه (Tag/Controller) به شکل زیر است:

1) کامندها به‌ترتیب: Create → Update → Patch → Delete
2) سپس کوئری‌ها به‌ترتیب: GetById → Paged → سایر فیلترها → در انتها GetAll

این ترتیب بر اساس نام اکشن/الگوی مسیر و متد HTTP تشخیص داده می‌شود و نیاز به تنظیم خاصی در سرویس‌ها ندارد.

## تنظیمات

در فایل `appsettings.json` سرویس مصرف‌کننده بخشی مشابه زیر داشته باشید:

```json
"Swagger": {
  "Enabled": true,
  "Ui": {
    "RoutePrefix": "swagger",
    "ExposeInProduction": false,
    "DisplayRequestDuration": true,
    "ShowExtensions": true,
    "EnableFilter": true,
    "DefaultModelsExpandDepth": -1
  }
}
```

- مقداردهی `Ui.ShowExtensions` باعث نمایش Vendor Extensions (من‌جمله `x-enumDescriptions`) در Swagger UI می‌شود.
- با تنظیم `Enabled = false` می‌توانید کل Swagger را برای محیط مشخصی غیرفعال کنید.

## نحوه استفاده

```csharp
builder.Services.AddSwaggerExtension(builder.Configuration, builder.Environment);
app.UseSwaggerExtension(builder.Environment);
```

این فراخوانی‌ها در `Program.cs` هر سرویس انجام می‌شوند و نیازی به هرگونه پیکربندی دستی داخل سرویس‌ها نیست.
