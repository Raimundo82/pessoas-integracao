namespace Pessoas.Integracao.Core.Application.DTOs;

public sealed record ImportPessoasResultDto(
    int TotalProcessed,
    int TotalAdded,
    int TotalUpdated
);