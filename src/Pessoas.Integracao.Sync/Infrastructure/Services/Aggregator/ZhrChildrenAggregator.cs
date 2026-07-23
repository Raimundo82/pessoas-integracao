using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.Aggregator;

public sealed class ZhrChildrenAggregator : IZhrChildrenAggregator
{
    public IReadOnlyList<ZhrSBaseModel[]> Aggregate<TOutput>(
        IReadOnlyList<TOutput> outputs)
        where TOutput : ZhrSBaseModelOutput
    {
        return [.. outputs
            .SelectMany(x => x.GetChildren())
            .GroupBy(x => x.GetType())
            .Select(g => g.ToArray())];
    }
}
