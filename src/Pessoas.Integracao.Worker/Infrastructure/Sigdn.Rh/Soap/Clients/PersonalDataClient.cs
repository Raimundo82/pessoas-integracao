using Microsoft.Extensions.Options;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;

public class PersonalDataClient(
        IOptions<DataSourceSettings> dataSourceSettings,
        ISoapChannelProvider<zhr_wsChannel> soapChannelProvider
    ) : IPersonalDataClient
{
    private readonly DataSourceSettings _settings = dataSourceSettings.Value;
    private readonly ISoapChannelProvider<zhr_wsChannel> _soapChannelProvider = soapChannelProvider;
    public async Task<ZhrSPessoaisOutput[]> GetPersonalDataAsync(PessoaImportKey[] importKey, CancellationToken cancellationToken)
    {
        var channel = _soapChannelProvider.CreateChannel(_settings.OutputUrl);
        var input = importKey.Select(k => new ZhrWsInputStruct { Empresa = _settings.Empresa, Numsap = k.ExternalId, Ni = k.Nii });

        var response = await channel.ZhrWsPersonalDataAsync(new ZhrWsPersonalDataRequest
        {
            ZhrWsPersonalData = new ZhrWsPersonalData { Input = [.. input] }
        });

        return response.ZhrWsPersonalDataResponse.Output ?? [];
    }
}