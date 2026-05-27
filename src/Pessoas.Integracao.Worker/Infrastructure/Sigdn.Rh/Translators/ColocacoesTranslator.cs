using System.Globalization;

using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Translators;

public class ColocacoesTranslator : IColocacoesTranslator
{
    public List<Colocacao> Translate(ZhrSAtribOrgOutput? output)
    {
        if (output?.AtribOrg is not { Length: > 0 })
            return [];

        var culture = CultureInfo.InvariantCulture;

        return [.. output.AtribOrg
            .DistinctBy(a => a.Datapresenta)
            .Select(a => new Colocacao
            {
                ExternalReference = new UnidadeExternaRef(a.Unid),
                Inicio = DateTime.Parse(a.Datapresenta, culture)
            })];
    }
}

