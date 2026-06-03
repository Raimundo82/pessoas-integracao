using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients.Deltas;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;

public class SigdnRhDeltasProvider(IDeltasClient deltasClient) : IPessoasChangedImportKeyProvider
{
    private readonly IDeltasClient _deltasClient = deltasClient;

    public async Task<IReadOnlyList<PessoaImportKey>> GetChangedImportKeysAsync(TimePeriod timePeriod, CancellationToken ct)
    {
        var personnelNumbers = await _deltasClient.GetDeltasAsync(timePeriod, ct);
        return personnelNumbers
            .Select(pernr => new PessoaImportKey(pernr.Ni, pernr.Pernr))
            .DistinctBy(k => k.Nii)
            .ToList();
    }
}
