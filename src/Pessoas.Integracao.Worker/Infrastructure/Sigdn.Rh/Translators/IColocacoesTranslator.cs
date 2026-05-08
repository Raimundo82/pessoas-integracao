using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Translators;

public interface IColocacoesTranslator
{
    public List<Colocacao> Translate(ZhrSTemposervOutput? output);
}
