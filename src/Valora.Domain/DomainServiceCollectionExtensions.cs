using Microsoft.Extensions.DependencyInjection;

namespace Valora.Domain;

public static class DomainServiceCollectionExtensions
{
    /// <summary>
    /// Keeps domain setup explicit at the composition root. Domain rules are pure and need no container services.
    /// </summary>
    public static IServiceCollection AddValoraDomain(this IServiceCollection services) => services;
}
