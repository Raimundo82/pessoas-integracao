using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;

public sealed class SigdnRhPessoasProvider : IPessoasDataProvider
{

    public Task<IReadOnlyList<Pessoa>> GetPessoasByImportKeysAsync(IReadOnlyList<PessoaImportKey> keys, CancellationToken ct)
    {
        return Task
            .FromResult((IReadOnlyList<Pessoa>)keys
                .Select(k => new Pessoa { NII = k.Nii, ExternalId = k.ExternalId })
                .ToList()
                .AsReadOnly());
    }
}