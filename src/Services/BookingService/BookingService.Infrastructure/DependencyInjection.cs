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
		// Database
		services.AddDbContext<BookingDbContext>(options =>
		    options.UseSqlServer(
		        configuration.GetConnectionString("BookingDb"),
		        b => b.MigrationsAssembly(typeof(BookingDbContext).Assembly.FullName)));

		// Repositories
		services.AddScoped<IBookingRepository, BookingRepository>();

		// RabbitMQ
		services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));
		services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

		// gRPC Client
		services.Configure<GrpcSettings>(configuration.GetSection("Grpc"));
		services.AddSingleton<IUserValidationService, UserGrpcClient>();

		// Outbox Processor (Background Service)
		services.AddHostedService<OutboxProcessor>();

		return services;
	}
}
