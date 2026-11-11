using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Valora.Application;

namespace Valora.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddValoraInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ValoraDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Valora")));
        services.AddSingleton<IMonthClock, SystemMonthClock>();
        services.AddScoped<IValoraService, ValoraService>();
        return services;
    }
}
