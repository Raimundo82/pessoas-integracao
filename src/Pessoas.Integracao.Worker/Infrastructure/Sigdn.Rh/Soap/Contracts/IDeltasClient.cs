using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Deltas;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;

public interface IDeltasClient
{
    Task<ZhrWsGetDeltasPernrOut[]> GetDeltasAsync(TimePeriod timeperiod, CancellationToken cancellationToken);
}