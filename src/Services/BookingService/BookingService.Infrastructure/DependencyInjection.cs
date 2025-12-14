using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Messaging;
using BookingService.Domain.Repositories;
using BookingService.Domain.Services;
using BookingService.Infrastructure.Grpc;
using BookingService.Infrastructure.Outbox;
using BookingService.Infrastructure.Persistence;

namespace BookingService.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		// Build sql connection string from environment variables
		var sqlHost = configuration["SQLSERVER_HOST"] ?? "localhost";
		var sqlPort = configuration["SQLSERVER_PORT"] ?? "1433";
		var sqlUser = configuration["SQLSERVER_USER"] ?? "sa";
		var sqlPassword = configuration["SA_PASSWORD"];
		var database = configuration["SQLSERVER_DB"] ?? "BookingDb";

		if (string.IsNullOrWhiteSpace(sqlPassword))
		{
			throw new InvalidOperationException(
				"SQL Server password not found. " +
				"Please set SA_PASSWORD environment variable.");
		}

		var sqlConnectionString = $"Server={sqlHost},{sqlPort};Database={database};User Id={sqlUser};Password={sqlPassword};TrustServerCertificate=true;";

		// Build rabbitMQ connection settings from environment variables
		var rabbitMqHost = configuration["RABBITMQ_HOST"] ?? "localhost";
		var rabbitMqPort = configuration["RABBITMQ_PORT"] ?? "5672";
		var rabbitMqUser = configuration["RABBITMQ_USER"] ?? "guest";
		var rabbitMqPassword = configuration["RABBITMQ_PASSWORD"];

		if (string.IsNullOrWhiteSpace(rabbitMqPassword))
		{
			throw new InvalidOperationException(
				"RabbitMQ password not found. " +
				"Please set RABBITMQ_PASSWORD environment variable.");
		}

		// Database
		services.AddDbContext<BookingDbContext>(options =>
		    options.UseSqlServer(
		        sqlConnectionString,
		        b => b.MigrationsAssembly(typeof(BookingDbContext).Assembly.FullName)));

		// Repositories
		services.AddScoped<IBookingRepository, BookingRepository>();

		// RabbitMQ
		services.Configure<RabbitMqSettings>(options =>
		{
			options.HostName = rabbitMqHost;
			options.Port = int.Parse(rabbitMqPort);
			options.UserName = rabbitMqUser;
			options.Password = rabbitMqPassword;
		});
		services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

		// gRPC Client
		services.Configure<GrpcSettings>(configuration.GetSection("Grpc"));
		services.AddSingleton<IUserValidationService, UserGrpcClient>();

		// Outbox Processor (Background Service)
		services.AddHostedService<OutboxProcessor>();

		return services;
	}
}
