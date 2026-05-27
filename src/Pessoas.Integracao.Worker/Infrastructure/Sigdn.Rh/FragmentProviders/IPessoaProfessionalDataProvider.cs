using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Fragments;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.FragmentProviders;

public interface IPessoaProfessionalDataProvider
{
    Task<Dictionary<PessoaImportKey, PessoaProfessionalDataFragment>> GetPessoaProfessionalDataAsync(IReadOnlyList<PessoaImportKey> importKeys, CancellationToken cancellationToken);

}
