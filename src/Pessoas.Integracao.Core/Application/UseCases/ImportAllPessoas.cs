using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Application.Contracts;

namespace Pessoas.Integracao.Core.Application.UseCases;

public sealed class ImportAllPessoas(IPessoaRepository pessoaRepository, IPessoasProvider pessoasProvider, IUnitOfWork unitOfWork)
{
    private readonly IPessoaRepository _pessoaRepository = pessoaRepository;
    private readonly IPessoasProvider _pessoasProvider = pessoasProvider;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task ExecuteAsync(CancellationToken ct)
    {
        var pessoas = await _pessoasProvider.GetPessoasAsync(ct);
        await _pessoaRepository.AddOrUpdateAllAsync(pessoas, ct);
        await _unitOfWork.CommitAsync(ct);
    }
}