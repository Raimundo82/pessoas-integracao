using Microsoft.Extensions.Options;

using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients.PersonnelNumbers;

public class PersonnelNumbersClient(
        IOptions<DataSourceSettings> dataSourceSettings,
        ISoapChannelProvider<zhr_wsChannel> soapChannelProvider)
        : SoapBaseClient<zhr_wsChannel>(soapChannelProvider), IPersonnelNumbersClient
{
    private readonly DataSourceSettings _settings = dataSourceSettings.Value;

    public async Task<ZhrSListapessoal[]> GetPersonnelNumbersAsync(CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async channel =>
        {
            var result = await channel
                .ZhrWsGetPernrAsync(new ZhrWsGetPernrRequest
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
                })
                .WaitAsync(cancellationToken);

            return result.ZhrWsGetPernrResponse.Output.FirstOrDefault()?.Pessoal ?? [];
        });
    }
}
