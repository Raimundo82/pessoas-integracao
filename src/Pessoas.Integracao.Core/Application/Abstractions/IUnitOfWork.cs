namespace Pessoas.Integracao.Core.Application.Abstractions;

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken ct);
}