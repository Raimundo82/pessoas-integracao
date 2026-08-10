using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.Contracts;

namespace Pessoas.Integracao.Analitica.Infrastructure.Transformers;

public interface IDataTransformer<TAnaliticaModel>
    where TAnaliticaModel : AnaliticaBaseModel, IAnaliticaModel
{
    IReadOnlyList<TAnaliticaModel> Transform(IReadOnlyList<IZhrOutput> zhrOutputs);
}
