using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;

public sealed class testProvider(IDeltasClient client) : IDeltasProvider
{
    private readonly IDeltasClient _client = client;
    public async Task<IReadOnlyList<Delta>> GetDeltasAsync(CancellationToken cancellationToken)
    {
        var result = await _client.GetDeltasAsync(cancellationToken);
        return [.. result.Select(pernr => new Delta
        {
            NII = pernr.Ni,
            ExternalId = pernr.Pernr
        })];
    }
}