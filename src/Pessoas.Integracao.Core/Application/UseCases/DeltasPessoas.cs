using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.UseCases;

public class DeltasPessoas(IPessoaRepository pessoaRepository, IPessoasDataProvider pessoasDataProvider, IPessoasDeltasKeyProvider pessoasDeltaKeyProvider, IPessoasDeltaDetector pessoasDeltaDetector, IUnitOfWork unitOfWork)
{
    private readonly IPessoaRepository _pessoaRepository = pessoaRepository;
    private readonly IPessoasDataProvider _pessoasDataProvider = pessoasDataProvider;
    private readonly IPessoasDeltasKeyProvider _pessoasDeltasKeyProvider = pessoasDeltaKeyProvider;
    private readonly IPessoasDeltaDetector _pessoasDeltaDetector = pessoasDeltaDetector;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task<DeltaPessoasResult> ExecuteAsync(DateTime startTimestamp, DateTime endTimestamp, CancellationToken ct)
    {
        var timePeriod = new TimePeriod(startTimestamp, endTimestamp);
        var pessoasDeltasKeys = await _pessoasDeltasKeyProvider.GetPessoasDeltasKeysAsync(timePeriod, ct);

        var pessoasImportKeys = pessoasDeltasKeys
                                .Select(k => new PessoaImportKey(k.Nii, k.ExternalId))
                                .ToList();
        var pessoasFromProvider = await _pessoasDataProvider.GetPessoasByImportKeysAsync(pessoasImportKeys, ct);

        var pessoasToUpsert = new List<Pessoa>();

        foreach (var pessoa in pessoasFromProvider)
        {
            var pessoaInRepoList = await _pessoaRepository.GetPessoaByImportKeyAsync(new PessoaImportKey(pessoa.NII, pessoa.ExternalId), ct);
            var pessoaInRepo = pessoaInRepoList.FirstOrDefault();

            if (pessoaInRepo is not null && await _pessoasDeltaDetector.IsPessoaChangedAsync(pessoa, pessoaInRepo, ct))
                pessoasToUpsert.Add(pessoa);
        }

        if (pessoasToUpsert.Count > 0)
        {
            await _pessoaRepository.UpsertAllAsync(pessoasToUpsert, ct);
            await _unitOfWork.CommitAsync(ct);
        }

        return new DeltaPessoasResult(
            pessoasToUpsert.Count,
            pessoasDeltasKeys.Count
        );
    }
}