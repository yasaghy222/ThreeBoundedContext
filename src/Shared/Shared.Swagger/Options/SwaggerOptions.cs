using System.Collections.Generic;
using System.Linq;

namespace Shared.Swagger.Options;

public class SwaggerOptions
{
    public bool Enabled { get; set; } = true;
    public string RouteTemplate { get; set; } = "swagger/{documentName}/swagger.json";
    public List<SwaggerDocumentOptions> Documents { get; set; } = new();
    public SwaggerSecurityOptions Security { get; set; } = new();
    public SwaggerUiOptions Ui { get; set; } = new();

    internal IEnumerable<SwaggerDocumentOptions> GetDocumentsOrDefault(string serviceName)
    {
        if (Documents is { Count: > 0 })
        {
            return Documents;
        }

        return new[]
        {
            new SwaggerDocumentOptions
            {
                Name = "v1",
                Title = string.IsNullOrWhiteSpace(serviceName) ? "API" : $"{serviceName} API",
                Version = "v1",
                Description = $"{serviceName} endpoints"
            }
        };
    }
}
