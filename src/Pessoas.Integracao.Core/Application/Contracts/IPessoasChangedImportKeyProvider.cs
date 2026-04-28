using Pessoas.Integracao.Core.Application.Models;

namespace Pessoas.Integracao.Core.Application.Contracts;

public interface IPessoasChangedImportKeyProvider
{
    Task<IReadOnlyList<PessoaImportKey>> GetChangedImportKeysAsync(TimePeriod timePeriod, CancellationToken ct);
}
