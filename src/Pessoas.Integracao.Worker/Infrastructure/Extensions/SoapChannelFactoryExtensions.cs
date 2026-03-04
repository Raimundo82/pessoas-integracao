using System.ServiceModel.Description;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;

namespace Pessoas.Integracao.Worker.Infrastructure.Extensions;

public static class SoapChannelFactoryExtensions
{
    public static IServiceCollection AddSoapChannelFactorySingleton<TChannel>(
    this IServiceCollection services, Func<DataSourceSettings, string> endpointSelector, params IEndpointBehavior[] behaviors)
    {
        services.TryAddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<DataSourceSettings>>().Value;
            var endpointUrl = endpointSelector(settings);

            return SoapChannelFactory.CreateChannelFactory<TChannel>(endpointUrl, behaviors);
        });
        return services;
    }
}