using System.Globalization;

using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Translators;

public class ColocacoesTranslator : IColocacoesTranslator
{
    public List<Colocacao> Translate(ZhrSTemposervOutput? output)
    {
        if (output?.TempoServico is not { Length: > 0 })
            return [];

        var culture = CultureInfo.InvariantCulture;

        return output.TempoServico
            .Select(tempoServico => new Colocacao
            {
                ExternalReference = new UnidadeExternaRef(tempoServico.Zzunid),

                Inicio = DateTime.Parse(
                    tempoServico.Datainicio,
                    culture
                ),

                Fim = string.IsNullOrWhiteSpace(tempoServico.Datafim)
                    ? null
                    : DateTime.Parse(
                        tempoServico.Datafim,
                        culture
                    )
            })
            .ToList();
    }
}

