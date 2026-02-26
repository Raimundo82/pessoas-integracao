using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;

public sealed class SigdnRhPessoasProvider(IExternalPersonnelNumberClient client) : IPessoasDataProvider, IPessoasImportKeyProvider
{
    private readonly IExternalPersonnelNumberClient _client = client;
    public async Task<IReadOnlyList<Pessoa>> GetPessoasAsync(CancellationToken cancellationToken)
    {
        var result = await _client.GetExternalPersonnelNumbersAsync(cancellationToken);
        return [.. result.Select(pernr => new Pessoa
        {
            NII = pernr.Ni,
            ExternalId = pernr.Numsap
        })];
    }

    public Task<IReadOnlyList<Pessoa>> GetPessoasByImportKeysAsync(IReadOnlyList<PessoaImportKey> keys, CancellationToken ct)
    {
        return Task
            .FromResult((IReadOnlyList<Pessoa>)keys
                .Select(k => new Pessoa { NII = k.Nii, ExternalId = k.ExternalId })
                .ToList()
                .AsReadOnly());
    }
    public async Task<IReadOnlyList<PessoaImportKey>> GetSourceImportKeysAsync(CancellationToken ct)
    {
        var result = await _client.GetExternalPersonnelNumbersAsync(ct);
        return result
          .Select(pernr => new PessoaImportKey(pernr.Ni, pernr.Numsap))
          .ToList()
          .AsReadOnly();
    }
}