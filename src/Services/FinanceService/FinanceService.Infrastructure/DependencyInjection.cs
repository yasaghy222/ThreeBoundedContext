using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FinanceService.Domain.Repositories;
using FinanceService.Infrastructure.Messaging;
using FinanceService.Infrastructure.Persistence;

namespace FinanceService.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		// Database
		services.AddDbContext<FinanceDbContext>(options =>
		    options.UseSqlServer(
		        configuration.GetConnectionString("FinanceDb"),
		        b => b.MigrationsAssembly(typeof(FinanceDbContext).Assembly.FullName)));

		// Repositories
		services.AddScoped<IInvoiceRepository, InvoiceRepository>();

		// RabbitMQ Consumer
		services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));
		services.AddHostedService<BookingCreatedConsumer>();

		return services;
	}
}
