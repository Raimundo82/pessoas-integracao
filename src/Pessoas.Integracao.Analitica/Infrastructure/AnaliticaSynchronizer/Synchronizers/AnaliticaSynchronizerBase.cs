using Pessoas.Integracao.Analitica.Application.Contracts;
using Pessoas.Integracao.Analitica.Infrastructure.Mappers;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;

public abstract class AnaliticaSynchronizerBase<TModel, TSource>(
    IEntityMapper<TModel> mapper,
    IAnaliticaRepository<TModel> repository) : IAnaliticaSynchronizer
    where TModel : AnaliticaBaseModel, IAnaliticaModel
    where TSource : ZhrSBaseModel
{
    public async Task SyncAsync(IZhrOutput input, CancellationToken ct)
    {
        var source = GetSourceCollection(input);
        if (source is null || source.Count == 0)
        {
            return;
        }

        var mapped = source.Select(a =>
        {
            var mappedModel = mapper.Map(a);
            mappedModel.UpdatedAt = input.UpdateAt;
            mappedModel.Numsap = input.ExternalId;
            return mappedModel;
        }).ToList();

        await repository.ReplaceMatchingByNiAsync(mapped, ct);
    }

    protected abstract IList<TSource>? GetSourceCollection(IZhrOutput input);
}
