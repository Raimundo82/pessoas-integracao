using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;

public class SigdnRhPessoasImportKeysProvider(IPersonnelNumbersClient personnelNumbersClient) : IPessoasImportKeyProvider
{
    private readonly IPersonnelNumbersClient _personnelNumbersClient = personnelNumbersClient;

    public async Task<IReadOnlyList<PessoaImportKey>> GetSourceImportKeysAsync(CancellationToken ct)
    {
        var personnelNumbers = await _personnelNumbersClient.GetPersonnelNumbersAsync(ct);
        return personnelNumbers
            .Select(pernr => new PessoaImportKey(pernr.Ni, pernr.Numsap))
            .ToList()
            .AsReadOnly();
    }
}
