using System.ServiceModel.Channels;

namespace Pessoas.Integracao.Sync.Infrastructure.Configuration;

public class ZhrWsSettings
{
    public const string SectionName = "ZhrWsSettings";
    public string Empresa { get; set; } = "3000";
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public ZhrEndpointSettings Endpoints { get; set; } = new ZhrEndpointSettings();
    public ZhrAuthenticationSettings Auth { get; set; } = new ZhrAuthenticationSettings();
    public WcfBindingSettings Binding { get; set; } = new WcfBindingSettings();
}

public class ZhrEndpointSettings
{
    public const string SectionName = "ZhrWsSettings:Endpoints";

    public string BaseUrl { get; set; } = string.Empty;
    public string DadosPath { get; set; } = string.Empty;
    public string DeltasPath { get; set; } = string.Empty;
    public string DescodifPath { get; set; } = string.Empty;
}

public class ZhrAuthenticationSettings
{
    public const string SectionName = "ZhrWsSettings:Auth";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class WcfBindingSettings
{
    public const string SectionName = "ZhrWsSettings:Binding";
    public string SoapVersion { get; set; } = MessageVersion.Soap11.ToString();
    public string Encoding { get; set; } = "utf-8";
    public int MaxBufferSize { get; set; } = int.MaxValue;
    public long MaxReceivedMessageSize { get; set; } = int.MaxValue;
    public int ReceiveTimeoutSeconds { get; set; } = 60;
    public int SendTimeoutSeconds { get; set; } = 60;
    public int OpenTimeoutSeconds { get; set; } = 60;
    public int CloseTimeoutSeconds { get; set; } = 60;
    public bool DecompressionEnabled { get; set; } = false;
    public bool UseDefaultWebProxy { get; set; } = false;
}
