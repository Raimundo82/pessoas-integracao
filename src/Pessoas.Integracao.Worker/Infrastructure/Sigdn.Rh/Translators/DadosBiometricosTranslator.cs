using Microsoft.Extensions.Options;

using Pessoas.Integracao.Core.Domain.Enums;
using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Translators;

public class DadosBiometricosTranslator(IOptions<SigdnRhExamesMedConfig> config) : IDadosBiometricosTranslator
{
    private readonly SigdnRhExamesMedConfig _config = config.Value;

    public DadosBiometricos Translate(ZhrSExamesMedOutput? output)
    {
        if (output?.Exames is not { Length: > 0 })
            return new DadosBiometricos();

        var exames = output.Exames.Where(e => e.Subty == _config.Subty);

        var altura = exames.LastOrDefault(e => e.AreaExame == _config.Altura)?.Valor;
        var corOlhos = exames.LastOrDefault(e => e.AreaExame == _config.CorOlhos)?.ModalDesc;
        var grupoSangue = exames.LastOrDefault(e => e.AreaExame == _config.GrupoSanguineo)?.ModalDesc;
        var rhesus = exames.LastOrDefault(e => e.AreaExame == _config.Rhesus)?.ModalDesc;

        return new DadosBiometricos
        {
            AlturaEmCm = altura,
            CorDosOlhos = corOlhos,
            TipoDeSangue = new TipoDeSangue
            {
                GrupoSanguineo = Enum.TryParse<GrupoSanguineo>(grupoSangue?.Trim(), ignoreCase: true, out var grupoSanguineo) ? grupoSanguineo : null,
                Rhesus = Enum.TryParse<Rhesus>(rhesus?.Trim(), ignoreCase: true, out var rhesusEnum) ? rhesusEnum : null
            }
        };
    }
}
