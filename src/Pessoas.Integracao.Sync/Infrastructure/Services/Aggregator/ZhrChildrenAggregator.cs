using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.Aggregator;

public sealed class ZhrChildrenAggregator : IZhrChildrenAggregator
{
    public IReadOnlyList<ZhrSBaseModel[]> Aggregate(IReadOnlyList<ZhrSBaseModelOutput> outputs)
    {
        return [.. outputs
            .SelectMany(x => x.GetChildrenFlattened())
            .GroupBy(x => x.GetType())
            .Select(g => g.ToArray())];
    }
}
