using Microsoft.EntityFrameworkCore;
using Serilog;
using FinanceService.Application;
using FinanceService.Infrastructure;
using FinanceService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Finance Service API", Version = "v1" });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("FinanceDb")!,
        name: "sqlserver",
        tags: new[] { "db", "sql" })
    .AddRabbitMQ(
        builder.Configuration.GetConnectionString("RabbitMq")!,
        name: "rabbitmq",
        tags: new[] { "messaging" });

var app = builder.Build();

// Migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseRouting();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
