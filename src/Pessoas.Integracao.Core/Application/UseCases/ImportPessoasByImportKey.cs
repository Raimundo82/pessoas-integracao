using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;

namespace Pessoas.Integracao.Core.Application.UseCases;

public class ImportPessoasByImportKey(
    IPessoaRepository pessoaRepository,
    IPessoasDataProvider pessoasDataProvider,
    IUnitOfWork unitOfWork)
{
    private readonly IPessoaRepository _pessoaRepository = pessoaRepository;
    private readonly IPessoasDataProvider _pessoasDataProvider = pessoasDataProvider;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ImportPessoasResult> ExecuteAsync(IReadOnlyList<PessoaImportKey> keys, CancellationToken ct)
    {
        var pessoas = await _pessoasDataProvider.GetPessoasByImportKeysAsync(keys, ct);
        var upsertResult = await _pessoaRepository.UpsertAllAsync(pessoas, ct);
        await _unitOfWork.CommitAsync(ct);
        return new ImportPessoasResult(pessoas.Count, upsertResult.TotalAdded, upsertResult.TotalUpdated);
    }

}