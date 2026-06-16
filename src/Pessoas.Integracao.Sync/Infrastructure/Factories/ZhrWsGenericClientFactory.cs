using System.ServiceModel;
using System.ServiceModel.Channels;

using Microsoft.Extensions.Options;

using Pessoas.Integracao.Sync.Infrastructure.Configuration;

namespace Pessoas.Integracao.Sync.Infrastructure.Factories;

public sealed class ZhrWsGenericClientFactory<TClient, TChannel>(
        IBindingFactory bindingFactory,
        IOptions<ZhrWsSettings> settings,
        Func<ZhrWsSettings, string> pathSelector,
        Func<Binding, EndpointAddress, TClient> zhrClientConstructor)
    : IZhrWsGenericClientFactory<TClient, TChannel>
    where TClient : ClientBase<TChannel>
    where TChannel : class
{
    private readonly ZhrWsSettings _settings = settings.Value;
    public TClient CreateClient()
    {
        var binding = bindingFactory.CreateBinding();
        var endpoint = new EndpointAddress(GetUrl(_settings.Endpoints.BaseUrl, pathSelector(_settings)));
        var client = zhrClientConstructor(binding, endpoint);
        client.ClientCredentials.UserName.UserName = _settings.Auth.Username;
        client.ClientCredentials.UserName.Password = _settings.Auth.Password;
        return client;
    }

    public static string GetUrl(string baseUrl, string path)
        => $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
}
