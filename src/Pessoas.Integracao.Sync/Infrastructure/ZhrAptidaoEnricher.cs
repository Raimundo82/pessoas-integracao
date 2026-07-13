namespace Pessoas.Integracao.Sync.Infrastructure;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Data;

public class ZhrAptidaoEnricher(ZhrSDbContext dbContext) : IZhrOutputsEnricher
{

    public async Task EnrichAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        IReadOnlyList<ZhrOutput> zhrOutputs,
        CancellationToken ct)
    {
        var niis = pessoaSyncRefs.Select(p => p.Ni).ToList();

        var aptidoes = await dbContext
            .Set<ZhrSAptidao>()
            .Where(item => niis.Contains(item.Ni))
            .ToListAsync(cancellationToken: ct);

        var aptidoesLookup = aptidoes.ToLookup(p => p.Ni);

        foreach (var output in zhrOutputs)
        {
            output.Aptidoes.AddRange(aptidoesLookup[output.Ni]);
        }
    }
}
