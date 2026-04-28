using Pessoas.Integracao.Core.Application.Models;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Correlation;

public interface ISoapResultCorrelator
{
    Dictionary<PessoaImportKey, TOutput?> CorrelateByKey<TOutput>(
        IReadOnlyList<PessoaImportKey> keys,
        IEnumerable<TOutput>? output,
        Func<TOutput, string> niiSelector)
        where TOutput : class;
}
