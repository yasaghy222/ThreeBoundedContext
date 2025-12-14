namespace Shared.Swagger.Options;

public class SwaggerDocumentOptions
{
    public string Name { get; set; } = "v1";
    public string? Title { get; set; }
    public string Version { get; set; } = "v1";
    public string? Description { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? LicenseName { get; set; }
    public string? LicenseUrl { get; set; }
}
