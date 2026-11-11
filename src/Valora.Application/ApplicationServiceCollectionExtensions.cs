using Microsoft.Extensions.DependencyInjection;

namespace Valora.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddValoraApplication(this IServiceCollection services)
    {
        services.AddSingleton<IMonthSummaryCalculator, MonthSummaryCalculator>();
        return services;
    }
}
