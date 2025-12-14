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
		// Database
		services.AddDbContext<UserDbContext>(options =>
		    options.UseSqlServer(
		        configuration.GetConnectionString("UserDb"),
		        b => b.MigrationsAssembly(typeof(UserDbContext).Assembly.FullName)));

		// Repositories
		services.AddScoped<IUserRepository, UserRepository>();

		// RabbitMQ
		services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));
		services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

		// Domain Event Dispatcher
		services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

		return services;
	}
}
