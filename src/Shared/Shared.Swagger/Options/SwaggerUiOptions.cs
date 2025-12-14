namespace Shared.Swagger.Options;

public class SwaggerUiOptions
{
    public bool Enable { get; set; } = true;
    public string RoutePrefix { get; set; } = "swagger";
    public bool ExposeInProduction { get; set; } = false;
    public bool DisplayRequestDuration { get; set; } = true;
    public bool ShowExtensions { get; set; } = false;
    public bool EnableFilter { get; set; } = true;
    public int DefaultModelsExpandDepth { get; set; } = -1;
}
