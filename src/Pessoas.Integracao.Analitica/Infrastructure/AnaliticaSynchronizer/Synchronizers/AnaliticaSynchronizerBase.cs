using Pessoas.Integracao.Analitica.Application.Contracts;
using Pessoas.Integracao.Analitica.Infrastructure.Mappers;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;

public abstract class AnaliticaSynchronizerBase<TAnaliticaModel, TZhrModel>(
    IEntityMapper<TAnaliticaModel> mapper,
    IAnaliticaRepository<TAnaliticaModel> repository) : IAnaliticaSynchronizer
    where TAnaliticaModel : AnaliticaBaseModel, IAnaliticaModel
    where TZhrModel : ZhrSBaseModel
{
    public async Task SyncAsync(IReadOnlyList<IZhrOutput> zhrOutputs, CancellationToken ct)
    {
        var analiticaDataSlices = zhrOutputs
            .SelectMany(output => (GetZhrOutputSlice(output) ?? [])
            .Select(outputDataSlice =>
            {
                var analiticaDataSlice = mapper.Map(outputDataSlice);
                ApplyZhrOutputFields(analiticaDataSlice, output);
                return analiticaDataSlice;
            }))
            .ToList();

        if (analiticaDataSlices.Count == 0)
        {
            return;
        }

        await repository.ReplaceMatchingByNiAsync(analiticaDataSlices, ct);
    }

    protected abstract IList<TZhrModel>? GetZhrOutputSlice(IZhrOutput input);

    protected virtual void ApplyZhrOutputFields(TAnaliticaModel analiticaDataSlice, IZhrOutput output)
    {
        analiticaDataSlice.UpdatedAt = output.UpdateAt;
        analiticaDataSlice.Numsap = output.ExternalId;
    }
}
