using Pessoas.Integracao.Analitica.Application.Contracts;
using Pessoas.Integracao.Analitica.Infrastructure.Transformers;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;

public abstract class BaseSynchronizer<TAnaliticaModel, TZhrModel>(
    IDataTransformer<TAnaliticaModel> transformer,
    IAnaliticaRepository<TAnaliticaModel> repository) : IAnaliticaSynchronizer
    where TAnaliticaModel : AnaliticaBaseModel, IAnaliticaModel
    where TZhrModel : ZhrSBaseModel
{
    public async Task SyncAsync(IReadOnlyList<IZhrOutput> zhrOutputs, CancellationToken ct)
    {
        if (zhrOutputs.Count == 0) return;
        var analiticaDataSlices = transformer.Transform(zhrOutputs);
        if (analiticaDataSlices.Count == 0) return;
        await repository.ReplaceMatchingByNiAsync(analiticaDataSlices, ct);
    }
}
