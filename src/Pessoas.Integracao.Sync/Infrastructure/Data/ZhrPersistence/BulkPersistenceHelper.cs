using System.Collections.Concurrent;
using System.Reflection;

using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Sync.Infrastructure.Models.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Data.ZhrPersistence;

internal static class BulkPersistenceHelper
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> InsertMethodsCache = new();
    private static readonly ConcurrentDictionary<Type, MethodInfo> DeleteMethodsCache = new();

    internal static Task DeleteAllUntypedAsync(ZhrSDbContext context, Array entityItems, CancellationToken ct)
    {
        var elementType = entityItems.GetType().GetElementType()
            ?? throw new InvalidOperationException("Array has no element type.");

        _ = context.Model.FindEntityType(elementType)
            ?? throw new InvalidOperationException($"Type '{elementType.Name}' is not mapped.");

        var method = DeleteMethodsCache.GetOrAdd(elementType, t =>
            typeof(BulkPersistenceHelper)
               .GetMethod(nameof(DeleteAllAsync), BindingFlags.Public | BindingFlags.Static)!
               .MakeGenericMethod(t));

        return (Task)method.Invoke(null, [context, ct])!;
    }

    public static Task<int> DeleteAllAsync<T>(ZhrSDbContext context, CancellationToken ct) where T : ZhrSBaseModel
        => context.Set<T>().ExecuteDeleteAsync(ct);

    internal static Task BulkInsertUntypedAsync(ZhrSDbContext context, Array entityItems, CancellationToken ct)
    {
        var elementType = entityItems.GetType().GetElementType()
            ?? throw new InvalidOperationException("Array has no element type.");

        _ = context.Model.FindEntityType(elementType)
            ?? throw new InvalidOperationException($"Type '{elementType.Name}' is not mapped.");

        var method = InsertMethodsCache.GetOrAdd(elementType, t =>
            typeof(DbContextBulkExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == nameof(DbContextBulkExtensions.BulkInsertAsync) && m.IsGenericMethod)
                .MakeGenericMethod(t));

        return (Task)method.Invoke(null, [context, entityItems, null, null, null, ct])!;
    }
}
