using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

using Riok.Mapperly.Abstractions;

namespace Pessoas.Integracao.Analitica.Infrastructure.Mappers;

[Mapper]
internal static partial class SharedMappingConfig
{
    [MapperIgnoreSource(nameof(ZhrSBaseModel.Id))]
    [MapperIgnoreTarget(nameof(IAnaliticaModel.Id))]
    [MapperIgnoreTarget(nameof(IAnaliticaModel.UpdatedAt))]
    public static partial void IgnoreCommonFields(ZhrSBaseModel source, AnaliticaBaseModel target);
}
