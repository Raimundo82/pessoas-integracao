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
    ) : IExamesMedClient
{
    private readonly DataSourceSettings _settings = dataSourceSettings.Value;
    private readonly ISoapChannelProvider<zhr_wsChannel> _soapChannelProvider = soapChannelProvider;
    private readonly ISoapResultCorrelator _soapResultCorrelator = soapResultCorrelator;
    public async Task<Dictionary<PessoaImportKey, ZhrSExamesMedOutput?>> GetExamesMedAsync(IReadOnlyList<PessoaImportKey> importKey, CancellationToken cancellationToken)
    {
        if (importKey.Count == 0) return [];

        var channel = _soapChannelProvider.CreateChannel();
        var input = importKey.Select(k => new ZhrWsInputStruct { Empresa = _settings.Empresa, Numsap = k.ExternalId, Ni = k.Nii });

        var response = await channel
            .ZhrWsExamesMedAsync(new ZhrWsExamesMedRequest
            {
                ZhrWsExamesMed = new ZhrWsExamesMed { Input = [.. input] }
            })
            .WaitAsync(cancellationToken);

        var output = response.ZhrWsExamesMedResponse.Output;

        return _soapResultCorrelator.CorrelateByKey(importKey, output, x => x.Ni);
    }
}
