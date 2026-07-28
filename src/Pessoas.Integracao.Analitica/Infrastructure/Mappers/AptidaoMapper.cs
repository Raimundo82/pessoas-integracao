using System.Globalization;

using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

using Riok.Mapperly.Abstractions;

namespace Pessoas.Integracao.Analitica.Infrastructure.Mappers;

[Mapper]
public sealed partial class AptidaoMapper : IEntityMapper<ZhrSAptidao, ZhrWsAptidaoAptidao>
{
    [FormatProvider(Default = true)]
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    [MapperIgnoreSource(nameof(ZhrSBaseModel.Id))]
    [MapperIgnoreTarget(nameof(IAnaliticaModel.Id))]
    [MapperIgnoreTarget(nameof(IAnaliticaModel.UpdatedAt))]
    [MapperIgnoreTarget(nameof(ZhrWsAptidaoAptidao.Numsap))]
    private partial ZhrWsAptidaoAptidao MapFields(ZhrSAptidao source);

    public ZhrWsAptidaoAptidao Map(ZhrSAptidao source) => MapFields(source);
}
