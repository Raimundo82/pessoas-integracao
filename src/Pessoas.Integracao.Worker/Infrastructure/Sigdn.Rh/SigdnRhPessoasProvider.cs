using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;

public sealed class SigdnRhPessoasProvider(IExternalPersonnelNumberClient client) : IPessoasProvider
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

    public Task<IReadOnlyList<Pessoa>> GetPessoasByImportKeysAsync(IReadOnlyList<PessoaImportKey> pessoaImportKeys, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<PessoaImportKey>> GetSourceImportKeysAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}