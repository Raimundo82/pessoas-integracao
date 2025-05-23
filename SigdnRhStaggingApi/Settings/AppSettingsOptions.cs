namespace SigdnRhStaggingApi.Settings;

public class AppSettingsOptions
{
    public const string AppSettings = "AppSettings";
    public string SubPath { get; set; } = string.Empty;
    public string ReadApiKey { get; set; } = string.Empty;
    public string WriteApiKey { get; set; } = string.Empty;
    public bool AllowMissingHttpContext { get; set; } = false;
}