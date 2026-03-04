using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Deltas;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddExternalSoapClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .Configure<DataSourceSettings>(configuration.GetSection(DataSourceSettings.SectionName))
            .AddSoapChannelFactorySingleton<zhr_wsChannel>(settings => settings.OutputUrl)
            .AddSoapChannelFactorySingleton<ZHR_WS_DELTASChannel>(settings => settings.DeltasUrl)
            .AddScoped<ISoapChannelProvider<zhr_wsChannel>, SoapChannelProvider<zhr_wsChannel>>()
            .AddScoped<ISoapChannelProvider<ZHR_WS_DELTASChannel>, SoapChannelProvider<ZHR_WS_DELTASChannel>>()
            .AddScoped<IExternalPersonnelNumberClient, ExternalPersonnelNumberClient>()
            .AddScoped<IPessoasDataProvider, SigdnRhPessoasProvider>()
            .AddScoped<IPessoasImportKeyProvider, SigdnRhPessoasProvider>();
    }
}