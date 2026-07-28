using System.Globalization;

using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

using Riok.Mapperly.Abstractions;

namespace Pessoas.Integracao.Analitica.Infrastructure.Mappers;

[Mapper]
public sealed partial class PessoaisMapper : IEntityMapper
{
    [FormatProvider(Default = true)]
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    [IncludeMappingConfiguration(nameof(@SharedMappingConfig.IgnoreCommonFields))]
    [MapperIgnoreTarget(nameof(IAnaliticaModel.Numsap))]
    private partial ZhrWsPersonalDataPessoai MapFields(ZhrSPessoais source);

    public AnaliticaBaseModel Map(ZhrSBaseModel source) => MapFields((ZhrSPessoais)source);
}
