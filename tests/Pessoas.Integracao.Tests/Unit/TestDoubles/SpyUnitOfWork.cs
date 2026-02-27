using Pessoas.Integracao.Core.Application.Abstractions;

namespace Pessoas.Integracao.Tests.Unit.TestDoubles;

public sealed class SpyUnitOfWork : IUnitOfWork
{
    public int CommitCalls { get; private set; }
    public CancellationToken? LastToken { get; private set; }

    public Task CommitAsync(CancellationToken ct)
    {
        CommitCalls++;
        LastToken = ct;
        return Task.CompletedTask;
    }
}