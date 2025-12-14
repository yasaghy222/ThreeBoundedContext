using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shared.Swagger.Filters;

internal sealed class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var enumType = GetEnumType(context.Type);
        if (enumType is null)
        {
            return;
        }

        var names = Enum.GetNames(enumType);
        if (names.Length == 0)
        {
            return;
        }

        var displayPairs = names
            .Select(name =>
            {
                var member = enumType.GetMember(name).FirstOrDefault();
                var display = member?.GetCustomAttribute<DisplayAttribute>()?.GetName();
                var value = Convert.ToInt32(Enum.Parse(enumType, name));
                var label = string.IsNullOrWhiteSpace(display) ? name : $"{name} ({display})";
                return $"{label}: {value}";
            })
            .ToArray();

        // Represent enums as strings in Swagger schema so dropdowns show names instead of numeric values
        schema.Type = "string";
        schema.Format = null;
        schema.Enum ??= new List<IOpenApiAny>();
        schema.Enum.Clear();
        foreach (var name in names)
        {
            schema.Enum.Add(new OpenApiString(name));
        }

        var enumDescription = $"مقادیر مجاز: {string.Join(", ", displayPairs)}";

        schema.Description = string.IsNullOrWhiteSpace(schema.Description)
            ? enumDescription
            : $"{schema.Description}{Environment.NewLine}{enumDescription}";

        var namesArray = new OpenApiArray();
        foreach (var n in names)
        {
            namesArray.Add(new OpenApiString(n));
        }

        schema.Extensions["x-enumNames"] = namesArray;

        var extensionArray = new OpenApiArray();
        foreach (var pair in displayPairs)
        {
            extensionArray.Add(new OpenApiString(pair));
        }

        schema.Extensions["x-enumDescriptions"] = extensionArray;
    }

    private static Type? GetEnumType(Type type)
    {
        if (type.IsEnum)
        {
            return type;
        }

        var underlyingType = Nullable.GetUnderlyingType(type);
        return underlyingType is { IsEnum: true } ? underlyingType : null;
    }
}
