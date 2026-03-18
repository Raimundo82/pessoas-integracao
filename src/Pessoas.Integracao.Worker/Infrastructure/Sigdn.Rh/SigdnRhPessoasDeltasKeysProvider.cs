using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;

public class SigdnRhPessoasDeltasKeysProvider(IDeltasClient deltasClient) : IPessoasDeltasKeyProvider
{
    private readonly IDeltasClient _deltasClient = deltasClient;

    public async Task<IReadOnlyList<PessoaDeltasKey>> GetPessoasDeltasKeysAsync(TimePeriod timePeriod, CancellationToken ct)
    {
        var personnelNumbers = await _deltasClient.GetDeltasAsync(timePeriod, ct);
        return personnelNumbers
            .Select(pernr => new PessoaDeltasKey(pernr.Ni, pernr.Pernr, pernr.Actio))
            .ToList()
            .AsReadOnly();
    }
}