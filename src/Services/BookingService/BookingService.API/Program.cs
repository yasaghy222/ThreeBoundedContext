using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Swagger;
using BookingService.Application;
using BookingService.Infrastructure;
using BookingService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configure to read from environment variables first, then appsettings
builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Controllers
builder.Services.AddControllers();
builder.Services.AddSwaggerExtension(builder.Configuration, builder.Environment);

// Health Checks
var sqlHost = builder.Configuration["SQLSERVER_HOST"] ?? "localhost";
var sqlPort = builder.Configuration["SQLSERVER_PORT"] ?? "1433";
var sqlUser = builder.Configuration["SQLSERVER_USER"] ?? "sa";
var sqlPassword = builder.Configuration["SA_PASSWORD"];
var database = builder.Configuration["SQLSERVER_DB"] ?? "BookingDb";
var sqlConnectionString = $"Server={sqlHost},{sqlPort};Database={database};User Id={sqlUser};Password={sqlPassword};TrustServerCertificate=true;";

var rabbitMqHost = builder.Configuration["RABBITMQ_HOST"] ?? "localhost";
var rabbitMqPort = builder.Configuration["RABBITMQ_PORT"] ?? "5672";
var rabbitMqUser = builder.Configuration["RABBITMQ_USER"] ?? "guest";
var rabbitMqPassword = builder.Configuration["RABBITMQ_PASSWORD"] ?? "guest";
var rabbitmqConnectionString = $"amqp://{rabbitMqUser}:{rabbitMqPassword}@{rabbitMqHost}:{rabbitMqPort}/";

builder.Services.AddHealthChecks()
    .AddSqlServer(
        sqlConnectionString,
        name: "sqlserver",
        tags: new[] { "db", "sql" })
    .AddRabbitMQ(
        rabbitmqConnectionString,
        name: "rabbitmq",
        tags: new[] { "messaging" });

var app = builder.Build();

// Migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerExtension(app.Environment);
}

app.UseSerilogRequestLogging();

app.UseRouting();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
