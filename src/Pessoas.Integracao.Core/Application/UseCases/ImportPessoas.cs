using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Application.Contracts;

namespace Pessoas.Integracao.Core.Application.UseCases;

public sealed class ImportPessoas(IPessoaRepository pessoaRepository, IPessoasDataProvider pessoasDataProvider, IPessoasImportKeyProvider pessoasImportKeyProvider, IUnitOfWork unitOfWork)
{
    private readonly IPessoaRepository _pessoaRepository = pessoaRepository;
    private readonly IPessoasDataProvider _pessoasDataProvider = pessoasDataProvider;
    private readonly IPessoasImportKeyProvider _pessoasImportKeyProvider = pessoasImportKeyProvider;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task ExecuteAsync(CancellationToken ct)
    {
        var existingPessoasImportKeys = await _pessoaRepository.GetExistingImportKeysAsync(ct);
        var sourcePessoasImportKeys = await _pessoasImportKeyProvider.GetSourceImportKeysAsync(ct);
        var distinctImportKeys = sourcePessoasImportKeys.UnionBy(existingPessoasImportKeys, key => key.Nii).ToArray().AsReadOnly();

        var pessoas = await _pessoasDataProvider.GetPessoasByImportKeysAsync(distinctImportKeys, ct);
        await _pessoaRepository.AddOrUpdateAllAsync(pessoas, ct);
        await _unitOfWork.CommitAsync(ct);
    }
}