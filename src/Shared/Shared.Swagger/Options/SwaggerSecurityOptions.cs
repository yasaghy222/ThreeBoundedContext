namespace Shared.Swagger.Options;

public class SwaggerSecurityOptions
{
    public bool EnableBearer { get; set; } = true;
    public string Scheme { get; set; } = "Bearer";
    public string BearerFormat { get; set; } = "JWT";
    public string Description { get; set; } = "JWT Authorization header using the Bearer scheme.";
    public string HeaderName { get; set; } = "Authorization";
}
