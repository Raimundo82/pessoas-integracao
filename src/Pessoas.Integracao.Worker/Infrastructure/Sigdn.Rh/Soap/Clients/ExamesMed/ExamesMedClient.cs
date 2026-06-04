using Microsoft.Extensions.Options;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Correlation;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients.ExamesMed;

public class ExamesMedClient(
        IOptions<DataSourceSettings> dataSourceSettings,
        ISoapChannelProvider<zhr_wsChannel> soapChannelProvider,
        ISoapResultCorrelator soapResultCorrelator
    ) : SoapBaseClient<zhr_wsChannel>(soapChannelProvider), IExamesMedClient
{
    private readonly DataSourceSettings _settings = dataSourceSettings.Value;
    private readonly ISoapResultCorrelator _soapResultCorrelator = soapResultCorrelator;
    public async Task<Dictionary<PessoaImportKey, ZhrSExamesMedOutput?>> GetExamesMedAsync(IReadOnlyList<PessoaImportKey> importKeys, CancellationToken cancellationToken)
    {
        return importKeys.Count == 0
            ? []
            : await ExecuteAsync(async channel =>
        {
            var input = importKeys.Select(k => new ZhrWsInputStruct { Empresa = _settings.Empresa, Numsap = k.ExternalId, Ni = k.Nii });
            var response = await channel
                .ZhrWsExamesMedAsync(new ZhrWsExamesMedRequest
                {
                    ZhrWsExamesMed = new ZhrWsExamesMed { Input = [.. input] }
                })
                .WaitAsync(cancellationToken);

            var output = response.ZhrWsExamesMedResponse.Output;

            return _soapResultCorrelator.CorrelateByKey(importKeys, output, x => x.Ni);
        });
    }
}
