using Pessoas.Integracao.Core.Application.Contracts;
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

    public async Task<IReadOnlyCollection<Pessoa>> GetPessoasByNiiAsync(IReadOnlyCollection<Pessoa> pessoas, CancellationToken cancellationToken)
    {
        var niis = pessoas
            .Select(p => p.NII)
            .ToArray();

        var semaphore = new SemaphoreSlim(5);

        // TODO: Refactor Parallel execution
        var tasks = niis
                    .Select(async nii =>
                            {
                                await semaphore.WaitAsync(cancellationToken);
                                try
                                {
                                    return await _client.GetExternalPersonnelNumberByNiiAsync(nii, cancellationToken);
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            });

        var results = await Task.WhenAll(tasks);

        return [.. results
                    .SelectMany(r => r)
                    .Select(pernr => new Pessoa
                    {
                        NII = pernr.Ni,
                        ExternalId = pernr.Numsap
                    })];
    }
}