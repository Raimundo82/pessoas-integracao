using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.ValueObjects;

namespace Pessoas.Integracao.Core.Application.Contracts;

public interface IDadosBiometricosProvider
{
    Task<IReadOnlyList<DadosBiometricos?>> GetDadosBiometricosByImportKeysAsync(IReadOnlyList<PessoaImportKey> keys, CancellationToken ct);
}