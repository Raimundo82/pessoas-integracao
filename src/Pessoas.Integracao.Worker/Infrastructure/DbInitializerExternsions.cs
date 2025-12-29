using Pessoas.Integracao.Core.Infrastructure.Data;

namespace Pessoas.Integracao.Worker.Infrastructure;

public static class DbInitializerExternsions
{
    public static async Task InitialiseDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<DbInitialiser>();

        await initialiser.InitialiseAsync();
    }
}