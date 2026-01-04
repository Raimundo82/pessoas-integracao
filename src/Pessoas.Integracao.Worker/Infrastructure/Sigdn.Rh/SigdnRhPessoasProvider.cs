using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;

public sealed class SigdnRhPessoasProvider(IExternalPersonnelNumberClient client) : IPessoasProvider
{
    private readonly IExternalPersonnelNumberClient _client = client;
    public async Task<IReadOnlyCollection<Pessoa>> GetPessoasAsync(CancellationToken cancellationToken)
    {
        var result = await _client.GetExternalPersonnelNumbersAsync(cancellationToken);
        return [.. result.Select(pernr => new Pessoa
        {
            NII = pernr.Ni,
            ExternalId = pernr.Numsap
        })];
    }
}