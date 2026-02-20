using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.DTOs;
using Pessoas.Integracao.Core.Application.Helper;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.Interfaces;

namespace Pessoas.Integracao.Core.Application.UseCases;

public sealed class ImportAllPessoas(IPessoaRepository pessoaRepository, IPessoasProvider pessoasProvider, IUnitOfWork unitOfWork)
{
    private readonly IPessoaRepository _pessoaRepository = pessoaRepository;
    private readonly IPessoasProvider _pessoasProvider = pessoasProvider;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task ExecuteAsync(CancellationToken ct)
    {
        var distinctImportNiis = await GetDistinctImportNiisAsync(ct);
        var pessoasImportUpdated = await _pessoasProvider.GetPessoasByNiiAsync(distinctImportNiis, ct);

        await _pessoaRepository.AddOrUpdateAllAsync(pessoasImportUpdated, ct);
        await _unitOfWork.CommitAsync(ct);
    }

    private async Task<IReadOnlyList<ImportNiiDto>> GetDistinctImportNiisAsync(CancellationToken ct)
    {
        var providerImportNiis = await _pessoasProvider.GetProviderImportNiisAsync(ct);

        var pessoasInRepository = await _pessoaRepository.GetAllAsync(ct);
        var repositoryImportNiis = await GetRepositoryImportNiisAsync(pessoasInRepository);

        return LogicOperationsHelper.UnionBy(providerImportNiis, repositoryImportNiis, p => p.Nii);
    }

    private static async Task<IReadOnlyList<ImportNiiDto>> GetRepositoryImportNiisAsync(IReadOnlyList<Pessoa> pessoasInRepository)
    {
        return pessoasInRepository
            .Select(p => p.NII)
            .Select(nii => new ImportNiiDto(nii))
            .ToList()
            .AsReadOnly();
    }
}