namespace Pessoas.Integracao.Core.Application.Helper;

public static class LogicOperationsHelper
{
    public static IReadOnlyCollection<T> UnionBy<T, TKey>(
    IReadOnlyCollection<T> first,
    IReadOnlyCollection<T>? second,
    Func<T, TKey> keySelector)
    {
        //first ??= Array.Empty<T>();
        //second ??= Array.Empty<T>();

        //if (first.Count == 0) return second;
        //if (second.Count == 0) return first;

        return first
        .Concat(second)
        .GroupBy(keySelector)
        .Select(g => g.First())
        .ToList();
    }
}