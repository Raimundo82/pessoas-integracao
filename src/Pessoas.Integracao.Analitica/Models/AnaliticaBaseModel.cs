namespace Pessoas.Integracao.Analitica.Models;

public interface IAnaliticaModel
{
    public int Id { get; set; }
    public string Ni { get; set; }
    public string? Numsap { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public abstract class AnaliticaBaseModel
{

    public int Id { get; set; }
    public required string Ni { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public partial class ZhrWsAptidaoAptidao : AnaliticaBaseModel, IAnaliticaModel { }
public partial class ZhrWsAtribOrgAtribOrg : AnaliticaBaseModel, IAnaliticaModel { }

