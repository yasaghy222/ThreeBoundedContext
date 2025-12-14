using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Swagger;
using UserService.Application;
using UserService.Infrastructure;
using UserService.Infrastructure.Grpc;
using UserService.Infrastructure.Persistence;

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

// gRPC
builder.Services.AddGrpc();

// Controllers
builder.Services.AddControllers();
builder.Services.AddSwaggerExtension(builder.Configuration, builder.Environment);

// Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("UserDb")!,
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
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerExtension(app.Environment);
}

app.UseSerilogRequestLogging();

app.UseRouting();

app.MapControllers();
app.MapGrpcService<UserGrpcService>();
app.MapHealthChecks("/health");

app.Run();
