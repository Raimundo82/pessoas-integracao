using System.ServiceModel.Channels;
using System.Text;

using Microsoft.Extensions.Options;

using Pessoas.Integracao.Sync.Infrastructure.Configuration;


namespace Pessoas.Integracao.Sync.Infrastructure.Factories;

public class BindingFactory(IOptions<ZhrWsSettings> settings) : IBindingFactory
{
    private readonly WcfBindingSettings _bindingSettings = settings.Value.Binding;
    public Binding CreateBinding()
    {
        return new CustomBinding(

             new TextMessageEncodingBindingElement(
                 ParseMessageVersion(_bindingSettings.SoapVersion),
                 GetEncodingOrDefault(_bindingSettings.Encoding)),
             new HttpTransportBindingElement
             {
                 MaxBufferSize = _bindingSettings.MaxBufferSize,
                 MaxReceivedMessageSize = _bindingSettings.MaxReceivedMessageSize,
                 DecompressionEnabled = _bindingSettings.DecompressionEnabled,
                 UseDefaultWebProxy = _bindingSettings.UseDefaultWebProxy
             })
        {
            CloseTimeout = TimeSpan.FromSeconds(TimeoutSecondsOrDefault(_bindingSettings.CloseTimeoutSeconds)),
            OpenTimeout = TimeSpan.FromSeconds(TimeoutSecondsOrDefault(_bindingSettings.OpenTimeoutSeconds)),
            ReceiveTimeout = TimeSpan.FromSeconds(TimeoutSecondsOrDefault(_bindingSettings.ReceiveTimeoutSeconds)),
            SendTimeout = TimeSpan.FromSeconds(TimeoutSecondsOrDefault(_bindingSettings.SendTimeoutSeconds))
        };
    }

    private static MessageVersion ParseMessageVersion(string input)
    {
        return input?.ToLowerInvariant() switch
        {
            "soap11" => MessageVersion.Soap11,
            "soap12" => MessageVersion.Soap12,
            _ => MessageVersion.Soap11,
        };
    }

    private static int TimeoutSecondsOrDefault(int seconds)
    {
        return seconds > 0 && seconds <= 600 ? seconds : 60;
    }

    private static Encoding GetEncodingOrDefault(string encodingName)
    {
        try
        {
            return Encoding.GetEncoding(encodingName);
        }
        catch (ArgumentException)
        {
            return Encoding.UTF8;
        }
    }
}
