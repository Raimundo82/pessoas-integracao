using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.DTOs;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Deltas;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;

public interface IDeltasClient
{
    Task<ZhrWsGetDeltasPernrOut[]> GetDeltasAsync(TimePeriodDto timeperiod, CancellationToken cancellationToken);
}