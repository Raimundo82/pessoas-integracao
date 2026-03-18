using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.UseCases;

public class DeltasPessoas(IPessoaRepository pessoaRepository, IPessoasDataProvider pessoasDataProvider, IPessoasDeltasKeyProvider pessoasDeltaKeyProvider, IUnitOfWork unitOfWork)
{
    private readonly IPessoaRepository _pessoaRepository = pessoaRepository;
    private readonly IPessoasDataProvider _pessoasDataProvider = pessoasDataProvider;
    private readonly IPessoasDeltasKeyProvider _pessoasDeltasKeyProvider = pessoasDeltaKeyProvider;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task<DeltaPessoasResult> ExecuteAsync(DateTime startTimestamp, DateTime endTimestamp, CancellationToken ct)
    {
        var timePeriod = new TimePeriod(startTimestamp, endTimestamp);
        var pessoasDeltasKeys = await _pessoasDeltasKeyProvider.GetPessoasDeltasKeysAsync(timePeriod, ct);

        // this mapping may be needed to be performed by a dedicated method of the DataProvider. 
        // this requires a change to the data provider interface
        var pessoasImportKeys = pessoasDeltasKeys
                        .Select(k => new PessoaImportKey(k.Nii, k.ExternalId))
                        .ToList();

        var pessoasFromProvider = await _pessoasDataProvider.GetPessoasByImportKeysAsync(pessoasImportKeys, ct);

        var pessoasToUpsert = new List<Pessoa>();

        foreach (var pessoa in pessoasFromProvider)
        {
            if (await IsPessoaChangedAsync(pessoa, ct))
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

    private async Task<bool> IsPessoaChangedAsync(Pessoa pessoa, CancellationToken ct)
    {
        var pessoaInRepo = await _pessoaRepository.GetPessoaByImportKeyAsync(new PessoaImportKey(pessoa.NII, pessoa.ExternalId), ct);

        // requires the merkle tree algorithm
        //var merkleTree1 = new PessoaMerkleTree(pessoa);
        //var merkleTree2 = new PessoaMerkleTree(pessoaInRepo);

        //return merkleTree1.Root.Hash != merkleTree2.Root.Hash;
        return true; //needs to be changed
    }
}