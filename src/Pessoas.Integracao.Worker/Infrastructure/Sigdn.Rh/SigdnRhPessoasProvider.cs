using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.FragmentProviders;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;

public sealed class SigdnRhPessoasProvider(IPessoaCoreDataProvider pessoaCoreDataProvider) : IPessoasDataProvider
{

    private readonly IPessoaCoreDataProvider _pessoaCoreDataProvider = pessoaCoreDataProvider;

    public async Task<IReadOnlyList<Pessoa>> GetPessoasByImportKeysAsync(IReadOnlyList<PessoaImportKey> keys, CancellationToken ct)
    {
        if (keys.Count == 0) return new List<Pessoa>().AsReadOnly();

        var coreDataFrags = await _pessoaCoreDataProvider.GetPessoaCoreDataAsync(keys, ct);
        return keys
            .Select(k => new Pessoa
            {
                NII = k.Nii,
                ExternalId = k.ExternalId,
                DadosPessoais = coreDataFrags[k].DadosPessoais,
                DadosBiometricos = coreDataFrags[k].DadosBiometricos
            })
            .ToList()
            .AsReadOnly();
    }
}