using Microsoft.Extensions.Options;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Deltas;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients.Deltas;

public class DeltasClient(
        IOptions<DataSourceSettings> dataSourceSettings,
        ISoapChannelProvider<ZHR_WS_DELTASChannel> soapChannelProvider)
        : SoapBaseClient<ZHR_WS_DELTASChannel>(soapChannelProvider), IDeltasClient
{
    private readonly DataSourceSettings _settings = dataSourceSettings.Value;

    public async Task<ZhrWsGetDeltasPernrOut[]> GetDeltasAsync(TimePeriod timeperiod, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async channel =>
        {
            var result = await channel
                .ZhrWsGetDeltasPernrAsync(new ZhrWsGetDeltasPernrRequest
                {
                    ZhrWsGetDeltasPernr = new ZhrWsGetDeltasPernr
                    {
                        Input = new ZhrWsGetDeltasPernrIn
                        {
                            Bukrs = _settings.Empresa,
                            Begda = timeperiod.StartAsString(),
                            Endda = timeperiod.EndAsString()
                        }
                    }
                })
                .WaitAsync(cancellationToken);

            return result.ZhrWsGetDeltasPernrResponse?.Output ?? [];
        });
    }
}
