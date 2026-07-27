using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrBaseModelOutputEnricher;

public interface IZhrBaseModelOutputEnricher
{
    Task EnrichAsync<T>(IEnumerable<T> outputs, DateTimeOffset updateTime, CancellationToken ct)
        where T : ZhrSBaseModelOutput, IOutputModel;
}
