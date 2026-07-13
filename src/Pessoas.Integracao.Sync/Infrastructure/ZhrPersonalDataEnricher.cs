namespace Pessoas.Integracao.Sync.Infrastructure;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Data;

public class ZhrPersonalDataEnricher(ZhrSDbContext dbContext) : IZhrOutputsEnricher
{

    public async Task EnrichAsync(IReadOnlyList<PessoaSyncRef> pessoaSyncRefs, IReadOnlyList<ZhrOutput> zhrOutputs, CancellationToken ct)
    {
        var niis = pessoaSyncRefs.Select(p => p.Ni);
        var pessoais = await dbContext.Set<ZhrSPessoais>().Where(item => niis.Contains(item.Ni)).ToListAsync(cancellationToken: ct);
        var familias = await dbContext.Set<ZhrSFamilia>().Where(item => niis.Contains(item.Ni)).ToListAsync(cancellationToken: ct);
        var outrosDados = await dbContext.Set<ZhrSOutrosdados>().Where(item => niis.Contains(item.Ni)).ToListAsync(cancellationToken: ct);
        var deficiencias = await dbContext.Set<ZhrSDeficiencias>().Where(item => niis.Contains(item.Ni)).ToListAsync(cancellationToken: ct);

        foreach (var output in zhrOutputs)
        {
            output.Pessoais.AddRange(pessoais.Where(p => p.Ni == output.Ni));
            output.Familias.AddRange(familias.Where(p => p.Ni == output.Ni));
            output.OutrosDados.AddRange(outrosDados.Where(p => p.Ni == output.Ni));
            output.Deficiencias.AddRange(deficiencias.Where(p => p.Ni == output.Ni));
        }
    }
}
