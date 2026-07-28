using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Infrastructure.Mappers;

public interface IEntityMapper<TTarget>
    where TTarget : AnaliticaBaseModel, IAnaliticaModel
{
    TTarget Map(ZhrSBaseModel source);
}
