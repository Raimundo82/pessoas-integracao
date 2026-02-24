using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.DTOs;

namespace Pessoas.Integracao.Core.Application.UseCases;

public class GetAllPessoas(IPessoaRepository pessoaRepository)
{
    private readonly IPessoaRepository _pessoaRepository = pessoaRepository;
    public async Task<IReadOnlyList<PessoaDto>> ExecuteAsync(CancellationToken ct)
    {
        var pessoas = await _pessoaRepository.GetAllAsync(ct);
        return [.. pessoas.Select(p => new PessoaDto(p.NII, p?.ExternalId))];
    }
}