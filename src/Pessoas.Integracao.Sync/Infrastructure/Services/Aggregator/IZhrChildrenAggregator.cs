using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.Aggregator;

public interface IZhrChildrenAggregator
{
    IReadOnlyList<ZhrSBaseModel[]> Aggregate<TOutput>(
        IReadOnlyList<TOutput> outputs)
        where TOutput : ZhrSBaseModelOutput;
}
