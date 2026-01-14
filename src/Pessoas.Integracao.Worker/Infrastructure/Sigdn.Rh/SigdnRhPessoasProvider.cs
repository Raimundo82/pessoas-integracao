using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.DTOs;
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

    public async Task<IReadOnlyList<Pessoa>> GetPessoasByNiiAsync(IReadOnlyList<ImportNiiDto> importNiis, CancellationToken cancellationToken)
    {
        var results = await _client.GetExternalPersonnelNumberByImportNiisAsync(importNiis, cancellationToken);

        return
        [
            .. results
                .Where(pernr => pernr.Msgty == "S")
                .Select(pernr => new Pessoa
                {
                    NII = pernr.Ni,
                    ExternalId = pernr.Numsap
                })
        ];
    }

    public async Task<IReadOnlyList<ImportNiiDto>> GetProviderImportNiisAsync(CancellationToken cancellationToken)
    {
        var result = await _client.GetExternalPersonnelNumbersAsync(cancellationToken);
        return [.. result.Select(pernr => new ImportNiiDto(pernr.Ni))];
    }
}