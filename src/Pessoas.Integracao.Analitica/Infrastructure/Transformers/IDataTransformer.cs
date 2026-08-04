using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Infrastructure.Transformers;

public interface IDataTransformer<TAnaliticaModel, TZhrModel>
    where TAnaliticaModel : AnaliticaBaseModel, IAnaliticaModel
    where TZhrModel : ZhrSBaseModel
{
    IReadOnlyList<TAnaliticaModel> Transform(IReadOnlyList<IZhrOutput> zhrOutputs);
}
