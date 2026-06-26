namespace Pessoas.Integracao.Sync.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

using Npgsql;

public static class DbContextExtensions
{
    public static async Task TruncateTableAsync(
        this DbContext context,
        Type entityType,
        bool cascade = true,
        CancellationToken cancellationToken = default)
    {
        var modelType = context.Model.FindEntityType(entityType) ?? throw new InvalidOperationException($"Type '{entityType.Name}' is not mapped.");
        var tableName = modelType.GetTableName() ?? throw new InvalidOperationException($"Type '{modelType.Name}' has no table name.");

        var quotedTableName = new NpgsqlCommandBuilder().QuoteIdentifier(tableName);
        var cascadeStr = cascade ? " CASCADE" : string.Empty;
        var sql = $"TRUNCATE TABLE {quotedTableName}{cascadeStr};";
        await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }


    public static async Task TruncateTableAsync<TEntity>(
        this DbContext context,
        bool cascade = true,
        CancellationToken cancellationToken = default)
        where TEntity : class
        => await context.TruncateTableAsync(typeof(TEntity), cascade, cancellationToken);
}
