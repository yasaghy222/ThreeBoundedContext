using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Messaging;
using UserService.Domain.Common;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Messaging;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		// Build sql connection string from environment variables
		var sqlHost = configuration["SQLSERVER_HOST"] ?? "localhost";
		var sqlPort = configuration["SQLSERVER_PORT"] ?? "1433";
		var sqlUser = configuration["SQLSERVER_USER"] ?? "sa";
		var sqlPassword = configuration["SA_PASSWORD"];
		var database = configuration["SQLSERVER_DB"] ?? "UserDb";

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
		services.AddDbContext<UserDbContext>(options =>
		    options.UseSqlServer(
		        sqlConnectionString,
		        b => b.MigrationsAssembly(typeof(UserDbContext).Assembly.FullName)));

		// Repositories
		services.AddScoped<IUserRepository, UserRepository>();

		// RabbitMQ
		services.Configure<RabbitMqSettings>(options =>
		{
			options.HostName = rabbitMqHost;
			options.Port = int.Parse(rabbitMqPort);
			options.UserName = rabbitMqUser;
			options.Password = rabbitMqPassword;
		});
		services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

		// Domain Event Dispatcher
		services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

		return services;
	}
}
