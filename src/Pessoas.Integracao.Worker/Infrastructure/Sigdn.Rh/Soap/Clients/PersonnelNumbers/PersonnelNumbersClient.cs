using Microsoft.Extensions.Options;

using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients.PersonnelNumbers;

public class PersonnelNumbersClient(
        IOptions<DataSourceSettings> dataSourceSettings,
        ISoapChannelProvider<zhr_wsChannel> soapChannelProvider)
        : IPersonnelNumbersClient
{
    private readonly DataSourceSettings _settings = dataSourceSettings.Value;
    private readonly ISoapChannelProvider<zhr_wsChannel> _soapChannelProvider = soapChannelProvider;
    public async Task<ZhrSListapessoal[]> GetPersonnelNumbersAsync(CancellationToken cancellationToken)
    {
        var channel = _soapChannelProvider.CreateChannel();

        var result = await channel.ZhrWsGetPernrAsync(new ZhrWsGetPernrRequest
        {
            ZhrWsGetPernr = new ZhrWsGetPernr
            {
                Input = [
                    new ZhrWsInputStru
                    {
                        Empresa = _settings.Empresa,
                        Dtreferencia = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd")
                    }
                ]
            }
        });
        return result.ZhrWsGetPernrResponse.Output.FirstOrDefault()?.Pessoal ?? [];
    }
}
