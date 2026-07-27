using Pessoas.Integracao.Analitica.Application.Contracts;
using Pessoas.Integracao.Analitica.Infrastructure.Mappers;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;

public sealed class AnaliticaAptidaoSyncronizer(
    IEntityMapper<ZhrSAptidao, ZhrWsAptidaoAptidao> mapper,
    IAnaliticaRepository<ZhrWsAptidaoAptidao> repository) : IAnaliticaSynchronizer
{
    public async Task SyncAsync(IZhrOutput input, CancellationToken ct)
    {
        var source = input.Aptidoes;
        if (source is null || source.Count == 0)
        {
            return;
        }

        var mapped = source.Select(a => mapper.Map(a, input.ExternalId)).ToList();
        await repository.ReplaceMatchingByNiAsync(mapped, ct);
    }
}
