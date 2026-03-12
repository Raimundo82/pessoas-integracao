namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;

public class SigdnRhExamesMedConfig
{
    public const string SectionName = "SigdnRh:ExamesMed";
    public string Subty { get; set; } = string.Empty;
    public string Altura { get; set; } = string.Empty;
    public string GrupoSanguineo { get; set; } = string.Empty;
    public string Rhesus { get; set; } = string.Empty;
    public string CorOlhos { get; set; } = string.Empty;
}