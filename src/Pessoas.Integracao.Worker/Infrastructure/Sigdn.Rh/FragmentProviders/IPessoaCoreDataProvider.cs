using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Fragments;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.FragmentProviders;

public interface IPessoaCoreDataProvider
{
    Task<Dictionary<PessoaImportKey, PessoaCoreDataFragment>> GetPessoaCoreDataAsync(IReadOnlyList<PessoaImportKey> importKeys, CancellationToken cancellationToken);
}