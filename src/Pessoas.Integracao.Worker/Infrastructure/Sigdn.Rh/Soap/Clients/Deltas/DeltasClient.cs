using Microsoft.Extensions.Options;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Deltas;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients.Deltas;

public class DeltasClient(
        IOptions<DataSourceSettings> dataSourceSettings,
        ISoapChannelProvider<ZHR_WS_DELTASChannel> soapChannelProvider)
        : IDeltasClient
{
    private readonly DataSourceSettings _settings = dataSourceSettings.Value;
    private readonly ISoapChannelProvider<ZHR_WS_DELTASChannel> _soapChannelProvider = soapChannelProvider;
    public async Task<ZhrWsGetDeltasPernrOut[]> GetDeltasAsync(TimePeriod timeperiod, CancellationToken cancellationToken)
    {
        var channel = _soapChannelProvider.CreateChannel();

        var result = await channel.ZhrWsGetDeltasPernrAsync(new ZhrWsGetDeltasPernrRequest
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
        });
        return result.ZhrWsGetDeltasPernrResponse?.Output ?? [];
    }
}
