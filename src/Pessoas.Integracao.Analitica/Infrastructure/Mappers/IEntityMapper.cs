using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Infrastructure.Mappers;

public interface IEntityMapper
{
    AnaliticaBaseModel Map(ZhrSBaseModel source);
}
