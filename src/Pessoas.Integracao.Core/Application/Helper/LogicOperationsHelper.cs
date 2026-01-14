namespace Pessoas.Integracao.Core.Application.Helper;

public static class LogicOperationsHelper
{
    public static IReadOnlyList<T> UnionBy<T, TKey>(
    IReadOnlyList<T> first,
    IReadOnlyList<T>? second,
    Func<T, TKey> keySelector)
    {
        return first
        .Concat(second)
        .GroupBy(keySelector)
        .Select(g => g.First())
        .ToList();
    }
}