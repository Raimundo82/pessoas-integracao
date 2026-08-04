using Pessoas.Integracao.Analitica.Infrastructure.Mappers;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Infrastructure.Transformers;

public abstract class BaseDataTransformer<TAnaliticaModel, TZhrModel>(
    IEntityMapper<TAnaliticaModel> mapper
) : IDataTransformer<TAnaliticaModel, TZhrModel>
    where TAnaliticaModel : AnaliticaBaseModel, IAnaliticaModel
    where TZhrModel : ZhrSBaseModel
{
    public IReadOnlyList<TAnaliticaModel> Transform(IReadOnlyList<IZhrOutput> zhrOutputs)
    {

        return [.. zhrOutputs
            .SelectMany(output => (GetZhrOutputSlice(output) ?? [])
                .Select(outputDataSlice =>
                {
                    var analiticaDataSlice = mapper.Map(outputDataSlice);
                    ApplyZhrOutputFields(analiticaDataSlice, output);
                    return analiticaDataSlice;
                }
            )
        )];
    }

    protected abstract IList<TZhrModel>? GetZhrOutputSlice(IZhrOutput output);

    protected virtual void ApplyZhrOutputFields(TAnaliticaModel analiticaDataSlice, IZhrOutput output)
    {
        analiticaDataSlice.UpdatedAt = output.UpdateAt;
        analiticaDataSlice.Numsap = output.ExternalId;
    }
}
