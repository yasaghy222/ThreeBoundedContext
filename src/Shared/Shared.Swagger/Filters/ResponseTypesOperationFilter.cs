using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shared.Swagger.Filters;

public class ResponseTypesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var httpMethod = context.ApiDescription.HttpMethod?.ToUpperInvariant();
        var actionName = context.MethodInfo.Name;
        var hasAuthorize = context.MethodInfo.DeclaringType?.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any() == true
                          || context.MethodInfo.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();
        var hasAllowAnonymous = context.MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any();

        // Clear existing responses to rebuild
        operation.Responses.Clear();

        switch (httpMethod)
        {
            case "GET":
                if (IsPaged(actionName))
                {
                    AddResponse(operation, "200", "لیست با موفقیت دریافت شد");
                    AddResponse(operation, "422", "پارامترهای صفحه‌بندی نامعتبر");
                }
                else if (IsGetAll(actionName))
                {
                    AddResponse(operation, "200", "لیست با موفقیت دریافت شد");
                }
                else
                {
                    AddResponse(operation, "200", "با موفقیت دریافت شد");
                    AddResponse(operation, "404", "یافت نشد");
                }
                break;

            case "POST":
                if (IsCreate(actionName))
                {
                    AddResponse(operation, "201", "با موفقیت ایجاد شد");
                }
                else
                {
                    AddResponse(operation, "200", "عملیات با موفقیت انجام شد");
                }
                AddResponse(operation, "400", "درخواست نامعتبر");
                break;

            case "PUT":
            case "PATCH":
                AddResponse(operation, "204", "با موفقیت ویرایش شد");
                AddResponse(operation, "400", "درخواست نامعتبر");
                AddResponse(operation, "404", "یافت نشد");
                break;

            case "DELETE":
                AddResponse(operation, "204", "با موفقیت حذف شد");
                AddResponse(operation, "404", "یافت نشد");
                break;

            default:
                AddResponse(operation, "200", "موفق");
                break;
        }

        // Add 401 for protected endpoints
        if (hasAuthorize && !hasAllowAnonymous)
        {
            AddResponse(operation, "401", "احراز هویت نشده");
            AddResponse(operation, "403", "دسترسی غیرمجاز");
        }

        // Always add 500
        AddResponse(operation, "500", "خطای سرور");
    }

    private static void AddResponse(OpenApiOperation operation, string statusCode, string description)
    {
        if (!operation.Responses.ContainsKey(statusCode))
        {
            operation.Responses.Add(statusCode, new OpenApiResponse { Description = description });
        }
    }

    private static bool IsPaged(string actionName) =>
        actionName.Contains("Paged", StringComparison.OrdinalIgnoreCase) ||
        actionName.Contains("Paginated", StringComparison.OrdinalIgnoreCase) ||
        actionName.Contains("List", StringComparison.OrdinalIgnoreCase);

    private static bool IsGetAll(string actionName) =>
        actionName.Equals("GetAll", StringComparison.OrdinalIgnoreCase) ||
        actionName.Equals("Get", StringComparison.OrdinalIgnoreCase);

    private static bool IsCreate(string actionName) =>
        actionName.StartsWith("Create", StringComparison.OrdinalIgnoreCase) ||
        actionName.StartsWith("Add", StringComparison.OrdinalIgnoreCase) ||
        actionName.StartsWith("Register", StringComparison.OrdinalIgnoreCase);
}
