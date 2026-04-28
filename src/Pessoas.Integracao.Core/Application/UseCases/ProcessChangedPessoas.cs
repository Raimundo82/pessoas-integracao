using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Core.Application.UseCases;

public class ProcessChangedPessoas(
    IPessoaRepository pessoaRepository,
    IPessoasDataProvider pessoasDataProvider,
    IPessoasChangedImportKeyProvider pessoasChangedImportKeyProvider,
    IPessoaChangeDetector pessoaChangeDetetor,
    IUnitOfWork unitOfWork)
{
    private readonly IPessoaRepository _pessoaRepository = pessoaRepository;
    private readonly IPessoasDataProvider _pessoasDataProvider = pessoasDataProvider;
    private readonly IPessoasChangedImportKeyProvider _pessoasChangedImportKeyProvider = pessoasChangedImportKeyProvider;
    private readonly IPessoaChangeDetector _pessoasChangeDetector = pessoaChangeDetetor;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task ExecuteAsync(TimePeriod timePeriod, CancellationToken ct)
    {
        var changedImportKeys = await _pessoasChangedImportKeyProvider.GetChangedImportKeysAsync(timePeriod, ct);

        var pessoasChanged = await _pessoasDataProvider.GetPessoasByImportKeysAsync(changedImportKeys, ct);

        var equivalentPessoasInRepo = await _pessoaRepository.GetPessoasByNiiAsync(pessoasChanged.Select(p => p.NII).ToList(), ct);

        var pessoasToUpsert = pessoasChanged
            .Where(changed => _pessoasChangeDetector
                    .GetChanges(changed, equivalentPessoasInRepo
                    .FirstOrDefault(p => p.NII == changed.NII) ?? new Pessoa { NII = changed.NII })
                    .HasChanges)
            .ToList();

        await _pessoaRepository.UpsertAllAsync(pessoasToUpsert, ct);
        await _unitOfWork.CommitAsync(ct);
    }
}
