namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;

public class DataSourceSettings
{
    public const string SectionName = "SigdnRh:DataSource";
    public string OutputUrl { get; set; } = string.Empty;
    public string DeltasUrl { get; set; } = string.Empty;
    public string DescodifUrl { get; set; } = string.Empty;
    public string Empresa { get; set; } = "3000";
    public string ClientUsername { get; set; } = string.Empty;
    public string ClientPassword { get; set; } = string.Empty;
    public int SendTimeoutMinutes { get; set; } = 1;
    public int ReceiveTimeoutMinutes { get; set; } = 1;
}
