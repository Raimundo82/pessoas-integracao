using System.Globalization;

using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

using Riok.Mapperly.Abstractions;

namespace Pessoas.Integracao.Analitica.Infrastructure.Mappers;

[Mapper]
public sealed partial class AptidaoMapper : IEntityMapper<ZhrWsAptidaoAptidao>
{
    [FormatProvider(Default = true)]
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    [IncludeMappingConfiguration(nameof(@SharedMappingConfig.IgnoreCommonFields))]
    [MapperIgnoreTarget(nameof(IAnaliticaModel.Numsap))]
    private partial ZhrWsAptidaoAptidao MapFields(ZhrSAptidao source);

    public ZhrWsAptidaoAptidao Map(ZhrSBaseModel source) => MapFields((ZhrSAptidao)source);

}
