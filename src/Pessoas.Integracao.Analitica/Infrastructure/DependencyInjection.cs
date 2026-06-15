using Pessoas.Integracao.Analitica.Application.Contracts;
using Pessoas.Integracao.Analitica.Infrastructure.Repositories;

namespace Pessoas.Integracao.Analitica.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IAnaliticaRepository<>), typeof(AnaliticaRepository<>));
        return services;
    }
}
