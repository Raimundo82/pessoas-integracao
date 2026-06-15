
using Pessoas.Integracao.Analitica.Infrastructure.Data;
using Pessoas.Integracao.Analitica.Infrastructure.Repositories;

using Pessoas.Integracao.Analitica.Models;
namespace Pessoas.Integracao.Analitica.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(sp =>
            new AnaliticaRepository<ZhrWsAptidaoAptidao>(
                sp.GetRequiredService<AnaliticaDbContext>()));

        services.AddScoped(sp =>
            new AnaliticaRepository<ZhrWsAtribOrgAtribOrg>(
                sp.GetRequiredService<AnaliticaDbContext>()));

        return services;
    }
}
