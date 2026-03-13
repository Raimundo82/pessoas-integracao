using Microsoft.Extensions.Options;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Correlation;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;

public class PersonalDataClient(
        IOptions<DataSourceSettings> dataSourceSettings,
        ISoapChannelProvider<zhr_wsChannel> soapChannelProvider,
        ISoapResultCorrelator soapResultCorrelator
    ) : IPersonalDataClient
{
    private readonly DataSourceSettings _settings = dataSourceSettings.Value;
    private readonly ISoapChannelProvider<zhr_wsChannel> _soapChannelProvider = soapChannelProvider;
    private readonly ISoapResultCorrelator _soapResultCorrelator = soapResultCorrelator;
    public async Task<Dictionary<PessoaImportKey, ZhrSPessoaisOutput?>> GetPersonalDataAsync(IReadOnlyList<PessoaImportKey> importKeys, CancellationToken cancellationToken)
    {
        if (importKeys.Count == 0) return [];

        var channel = _soapChannelProvider.CreateChannel();
        var input = importKeys.Select(k => new ZhrWsInputStruct { Empresa = _settings.Empresa, Numsap = k.ExternalId, Ni = k.Nii });

        var response = await channel.ZhrWsPersonalDataAsync(new ZhrWsPersonalDataRequest
        {
            ZhrWsPersonalData = new ZhrWsPersonalData { Input = [.. input] }
        });

        var output = response.ZhrWsPersonalDataResponse.Output;

        return _soapResultCorrelator.CorrelateByKey(importKeys, output, x => x.Ni);
    }
}