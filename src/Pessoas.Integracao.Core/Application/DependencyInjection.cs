using Microsoft.Extensions.DependencyInjection;

using Pessoas.Integracao.Core.Application.UseCases;

namespace Pessoas.Integracao.Core.Application;

public static class DependecyInjections
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        return services.AddScoped<ImportPessoas>()
            .AddScoped<GetAllPessoas>();
    }
}