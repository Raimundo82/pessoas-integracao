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

    [MapperIgnoreSource(nameof(ZhrSAptidao.Id))]
    [MapperIgnoreTarget(nameof(ZhrWsAptidaoAptidao.Id))]
    [MapperIgnoreTarget(nameof(ZhrWsAptidaoAptidao.UpdatedAt))]
    [MapperIgnoreTarget(nameof(ZhrWsAptidaoAptidao.Numsap))]
    private partial ZhrWsAptidaoAptidao MapFields(ZhrSAptidao source);

    public ZhrWsAptidaoAptidao Map(ZhrSAptidao source) => MapFields(source);
}
