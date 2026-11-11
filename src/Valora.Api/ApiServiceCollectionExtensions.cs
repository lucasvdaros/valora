using Microsoft.Extensions.DependencyInjection;

namespace Valora.Api;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddValoraApi(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddProblemDetails();
        return services;
    }
}
