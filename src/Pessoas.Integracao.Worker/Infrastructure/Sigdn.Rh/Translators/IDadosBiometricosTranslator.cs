using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Translators;

public interface IDadosBiometricosTranslator
{
    DadosBiometricos Translate(ZhrSExamesMedOutput? output);
}