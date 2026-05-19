using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;

namespace Pessoas.Integracao.Core.Application.UseCases;

public sealed class ImportPessoas(
    IPessoaRepository pessoaRepository,
    IPessoasDataProvider pessoasDataProvider,
    IPessoasImportKeyProvider pessoasImportKeyProvider
    )
{
    private readonly IPessoaRepository _pessoaRepository = pessoaRepository;
    private readonly IPessoasDataProvider _pessoasDataProvider = pessoasDataProvider;
    private readonly IPessoasImportKeyProvider _pessoasImportKeyProvider = pessoasImportKeyProvider;
    public async Task<ImportPessoasResult> ExecuteAsync(CancellationToken ct)
    {
        var existingPessoasImportKeys = await _pessoaRepository.GetExistingImportKeysAsync(ct);
        var sourcePessoasImportKeys = await _pessoasImportKeyProvider.GetSourceImportKeysAsync(ct);
        var distinctImportKeys = sourcePessoasImportKeys.UnionBy(existingPessoasImportKeys, key => key.Nii).ToArray().AsReadOnly();
        var pessoas = await _pessoasDataProvider.GetPessoasByImportKeysAsync(distinctImportKeys, ct);
        await _pessoaRepository.ReplaceAllAsync(pessoas, ct);
        return new ImportPessoasResult(pessoas.Count);
    }
}
