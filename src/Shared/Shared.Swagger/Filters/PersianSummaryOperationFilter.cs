using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shared.Swagger.Filters;

/// <summary>
/// Generates bilingual (Persian + English) summary and description for Swagger operations.
/// Persian goes in Summary, English goes in Description.
/// </summary>
public class PersianSummaryOperationFilter : IOperationFilter
{
    private static readonly Dictionary<string, (string Persian, string English)> ActionTranslations = new(StringComparer.OrdinalIgnoreCase)
    {
        // Get operations
        ["GetById"] = ("دریافت بر اساس شناسه", "Get by ID"),
        ["GetByPhone"] = ("دریافت بر اساس شماره تلفن", "Get by phone number"),
        ["GetByCode"] = ("دریافت بر اساس کد", "Get by code"),
        ["GetPaged"] = ("دریافت لیست صفحه‌بندی شده", "Get paginated list"),
        ["GetAll"] = ("دریافت همه", "Get all"),
        ["Get"] = ("دریافت", "Get"),
        ["GetBasket"] = ("دریافت سبد خرید", "Get basket"),
        ["GetCurrent"] = ("دریافت جاری", "Get current"),
        ["Search"] = ("جستجو", "Search"),
        ["Find"] = ("یافتن", "Find"),
        ["List"] = ("لیست", "List"),

        // Create operations
        ["Create"] = ("ایجاد", "Create"),
        ["Add"] = ("افزودن", "Add"),
        ["AddItem"] = ("افزودن کالا", "Add item"),
        ["Register"] = ("ثبت‌نام", "Register"),
        ["Insert"] = ("درج", "Insert"),

        // Update operations
        ["Update"] = ("ویرایش", "Update"),
        ["UpdateItem"] = ("ویرایش کالا", "Update item"),
        ["Edit"] = ("ویرایش", "Edit"),
        ["Modify"] = ("تغییر", "Modify"),
        ["Patch"] = ("به‌روزرسانی جزئی", "Partial update"),
        ["Set"] = ("تنظیم", "Set"),
        ["SetAddress"] = ("تنظیم آدرس", "Set address"),
        ["SetDeliveryMethod"] = ("تنظیم روش ارسال", "Set delivery method"),
        ["SetPaymentMethod"] = ("تنظیم روش پرداخت", "Set payment method"),
        ["ApplyCoupon"] = ("اعمال کوپن", "Apply coupon"),

        // Delete operations
        ["Delete"] = ("حذف", "Delete"),
        ["Remove"] = ("حذف", "Remove"),
        ["RemoveItem"] = ("حذف کالا", "Remove item"),
        ["Clear"] = ("پاک‌سازی", "Clear"),

        // Auth operations
        ["Login"] = ("ورود", "Login"),
        ["Logout"] = ("خروج", "Logout"),
        ["Send"] = ("ارسال", "Send"),
        ["Verify"] = ("تأیید", "Verify"),
        ["Refresh"] = ("تازه‌سازی توکن", "Refresh token"),
        ["Revoke"] = ("ابطال", "Revoke"),
        ["Assign"] = ("تخصیص", "Assign"),
        ["Unassign"] = ("لغو تخصیص", "Unassign"),

        // Order/Invoice operations
        ["Checkout"] = ("تکمیل خرید", "Checkout"),
        ["Cancel"] = ("لغو", "Cancel"),
        ["Confirm"] = ("تأیید", "Confirm"),
        ["Pay"] = ("پرداخت", "Pay"),
        ["Start"] = ("شروع", "Start"),
        ["Callback"] = ("بازگشت از درگاه", "Payment callback"),
        ["Refund"] = ("استرداد", "Refund"),

        // Sync operations
        ["Sync"] = ("همگام‌سازی", "Sync"),
        ["Warmup"] = ("گرم‌کردن کش", "Cache warmup"),

        // Status endpoint
        ["Status"] = ("وضعیت", "Status"),
        ["Health"] = ("سلامت", "Health"),
    };

    private static readonly Dictionary<string, (string Persian, string English)> ControllerTranslations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Customer"] = ("مشتری", "Customer"),
        ["CustomerCategory"] = ("دسته‌بندی مشتری", "Customer Category"),
        ["Address"] = ("آدرس", "Address"),
        ["Phone"] = ("تلفن", "Phone"),
        ["City"] = ("شهر", "City"),
        ["Province"] = ("استان", "Province"),
        ["Country"] = ("کشور", "Country"),
        ["Identity"] = ("هویت", "Identity"),
        ["User"] = ("کاربر", "User"),
        ["Role"] = ("نقش", "Role"),
        ["Permission"] = ("دسترسی", "Permission"),
        ["Client"] = ("کلاینت", "Client"),
        ["Scope"] = ("محدوده", "Scope"),
        ["Tenant"] = ("تننت", "Tenant"),
        ["Otp"] = ("رمز یکبار مصرف", "OTP"),
        ["Catalog"] = ("کاتالوگ", "Catalog"),
        ["Product"] = ("محصول", "Product"),
        ["Item"] = ("کالا", "Item"),
        ["Items"] = ("کالاها", "Items"),
        ["Unit"] = ("واحد", "Unit"),
        ["Currency"] = ("ارز", "Currency"),
        ["Stock"] = ("انبار", "Stock"),
        ["SaleType"] = ("نوع فروش", "Sale Type"),
        ["Price"] = ("قیمت", "Price"),
        ["Order"] = ("سفارش", "Order"),
        ["Quotation"] = ("پیش‌فاکتور", "Quotation"),
        ["Basket"] = ("سبد خرید", "Basket"),
        ["MeBasket"] = ("سبد خرید", "My Basket"),
        ["Invoice"] = ("فاکتور", "Invoice"),
        ["Receipt"] = ("رسید", "Receipt"),
        ["Payment"] = ("پرداخت", "Payment"),
        ["Notification"] = ("اعلان", "Notification"),
        ["Sms"] = ("پیامک", "SMS"),
        ["Me"] = ("پروفایل من", "My Profile"),
        ["MeOrder"] = ("سفارش من", "My Order"),
        ["MeInvoice"] = ("فاکتور من", "My Invoice"),
        ["MePayment"] = ("پرداخت من", "My Payment"),
        ["MeAddress"] = ("آدرس من", "My Address"),
        ["Sep"] = ("سپ", "Sep Gateway"),
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var actionName = context.MethodInfo.Name;
        var controllerName = context.MethodInfo.DeclaringType?.Name.Replace("Controller", "") ?? "";

        var (persian, english) = BuildSummary(actionName, controllerName);

        // Set Persian as Summary (shown in Swagger UI as main title)
        if (string.IsNullOrWhiteSpace(operation.Summary) && !string.IsNullOrWhiteSpace(persian))
        {
            operation.Summary = persian;
        }

        // Set English as Description (shown as additional info)
        if (string.IsNullOrWhiteSpace(operation.Description) && !string.IsNullOrWhiteSpace(english))
        {
            operation.Description = english;
        }
    }

    private static (string Persian, string English) BuildSummary(string actionName, string controllerName)
    {
        var (actionPersian, actionEnglish) = TranslateAction(actionName);
        var (controllerPersian, controllerEnglish) = TranslateController(controllerName);

        if (string.IsNullOrWhiteSpace(actionPersian))
        {
            return (string.Empty, string.Empty);
        }

        if (string.IsNullOrWhiteSpace(controllerPersian))
        {
            return (actionPersian, actionEnglish);
        }

        return ($"{actionPersian} {controllerPersian}", $"{actionEnglish} {controllerEnglish}");
    }

    private static (string Persian, string English) TranslateAction(string actionName)
    {
        // Direct match
        if (ActionTranslations.TryGetValue(actionName, out var translation))
        {
            return translation;
        }

        // Try to find prefix match
        foreach (var (key, value) in ActionTranslations.OrderByDescending(x => x.Key.Length))
        {
            if (actionName.StartsWith(key, StringComparison.OrdinalIgnoreCase))
            {
                var suffix = actionName[key.Length..];
                if (!string.IsNullOrEmpty(suffix))
                {
                    var (suffixPersian, suffixEnglish) = TranslateController(suffix);
                    if (!string.IsNullOrWhiteSpace(suffixPersian))
                    {
                        return ($"{value.Persian} {suffixPersian}", $"{value.English} {suffixEnglish}");
                    }
                }
                return value;
            }
        }

        // Convert PascalCase to readable
        var readable = SplitPascalCase(actionName);
        return (readable, readable);
    }

    private static (string Persian, string English) TranslateController(string controllerName)
    {
        if (ControllerTranslations.TryGetValue(controllerName, out var translation))
        {
            return translation;
        }

        // Try compound names (e.g., CustomerCategory)
        foreach (var (key, value) in ControllerTranslations.OrderByDescending(x => x.Key.Length))
        {
            if (controllerName.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
        }

        return (string.Empty, string.Empty);
    }

    private static string SplitPascalCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        return Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
    }
}
