using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Domain.Interfaces;

namespace Pessoas.Integracao.Core.Application.UseCases;

public sealed class ImportAllPessoas(IPessoaRepository pessoaRepository, IPessoasProvider pessoasSource, IUnitOfWork unitOfWork)
{
    private readonly IPessoaRepository _pessoaRepository = pessoaRepository;
    private readonly IPessoasProvider _pessoasSource = pessoasSource;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task ExecuteAsync(CancellationToken ct)
    {
        var pessoas = await _pessoasSource.GetPessoasAsync(ct);
        await _pessoaRepository.ClearAllAsync(ct);
        await _pessoaRepository.AddRangeAsync(pessoas, ct);
        await _unitOfWork.CommitAsync(ct);
    }
}