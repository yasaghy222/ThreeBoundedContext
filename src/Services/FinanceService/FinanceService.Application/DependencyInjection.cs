using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        
        return services;
    }
}
