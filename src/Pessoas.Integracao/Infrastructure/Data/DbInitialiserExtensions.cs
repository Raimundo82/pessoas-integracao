namespace Pessoas.Integracao.Infrastructure.Data;

public static class DbInitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<DbInitialiser>();

        await initialiser.InitialiseAsync();
    }

}