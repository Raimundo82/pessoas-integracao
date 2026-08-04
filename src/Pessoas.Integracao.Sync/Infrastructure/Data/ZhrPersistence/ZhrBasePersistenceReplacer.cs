using Microsoft.Extensions.Logging;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Infrastructure.Contracts;

namespace Pessoas.Integracao.Sync.Infrastructure.Data.ZhrPersistence;

public abstract class ZhrBasePersistenceReplacer(ZhrSDbContext dbContext, ILogger logger) : IZhrPersistenceReplacer
{
    protected ZhrSDbContext DbContext { get; } = dbContext;
    protected ILogger Logger { get; } = logger;

    public async Task<bool> ExecuteAsync<T>(
        IReadOnlyList<T> roots,
        IReadOnlyList<ZhrSBaseModel[]> children,
        CancellationToken ct
    ) where T : ZhrSBaseModelOutput, IOutputModel
    {
        await using var transaction = await DbContext.Database.BeginTransactionAsync(ct);
        try
        {
            await ExecuteReplaceAsync<T>(roots, children, ct);
            await transaction.CommitAsync(ct);

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Persistence replace failed for {Type}", typeof(T).Name);
            await transaction.RollbackAsync(ct);

            return false;
        }
    }

    protected abstract Task ExecuteReplaceAsync<T>(
        IReadOnlyList<T> roots,
        IReadOnlyList<ZhrSBaseModel[]> children,
        CancellationToken ct
    ) where T : ZhrSBaseModelOutput, IOutputModel;
}
