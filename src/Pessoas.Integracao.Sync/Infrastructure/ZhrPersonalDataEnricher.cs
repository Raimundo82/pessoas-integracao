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
        var niis = pessoaSyncRefs.Select(p => p.Ni).ToList();
        var pessoaisTask = dbContext.Set<ZhrSPessoais>().Where(i => niis.Contains(i.Ni)).ToListAsync(ct);
        var familiasTask = dbContext.Set<ZhrSFamilia>().Where(i => niis.Contains(i.Ni)).ToListAsync(ct);
        var outrosDadosTask = dbContext.Set<ZhrSOutrosdados>().Where(i => niis.Contains(i.Ni)).ToListAsync(ct);
        var deficienciasTask = dbContext.Set<ZhrSDeficiencias>().Where(i => niis.Contains(i.Ni)).ToListAsync(ct);

        await Task.WhenAll(pessoaisTask, familiasTask, outrosDadosTask, deficienciasTask);

        var pessoaisLookup = pessoaisTask.Result.ToLookup(p => p.Ni);
        var familiasLookup = familiasTask.Result.ToLookup(f => f.Ni);
        var outrosDadosLookup = outrosDadosTask.Result.ToLookup(o => o.Ni);
        var deficienciasLookup = deficienciasTask.Result.ToLookup(d => d.Ni);

        foreach (var output in zhrOutputs)
        {
            output.Pessoais.AddRange(pessoaisLookup[output.Ni]);
            output.Familias.AddRange(familiasLookup[output.Ni]);
            output.OutrosDados.AddRange(outrosDadosLookup[output.Ni]);
            output.Deficiencias.AddRange(deficienciasLookup[output.Ni]);
        }
    }
}
