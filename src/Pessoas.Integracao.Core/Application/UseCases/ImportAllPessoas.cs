using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Helper;

using Pessoas.Integracao.Core.Domain.Interfaces;

namespace Pessoas.Integracao.Core.Application.UseCases;

public sealed class ImportAllPessoas(IPessoaRepository pessoaRepository, IPessoasProvider pessoasProvider, IUnitOfWork unitOfWork)
{
    private readonly IPessoaRepository _pessoaRepository = pessoaRepository;
    private readonly IPessoasProvider _pessoasProvider = pessoasProvider;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task ExecuteAsync(CancellationToken ct)
    {
        var pessoasInDb = await _pessoaRepository.GetAllAsync(ct);
        var pessoasInSource = await _pessoasProvider.GetPessoasAsync(ct);
        //var pessoasNonActiveInDb = pessoasInDb
        //                            .Where(db => !pessoasInSource.Any(src => src.NII == db.NII))
        //                            .ToArray();

        var pessoasInDbUpdated = await _pessoasProvider.GetPessoasByNiiAsync(pessoasInDb, ct);
        var pessoas = LogicOperationsHelper.UnionBy(pessoasInSource, pessoasInDbUpdated, p => p.NII);

        await _pessoaRepository.AddOrUpdateAllAsync(pessoas, ct);
        await _unitOfWork.CommitAsync(ct);
    }
}