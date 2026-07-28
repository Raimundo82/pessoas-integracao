namespace Pessoas.Integracao.Analitica.Models;

public interface IAnaliticaModel
{
    int Id { get; set; }
    string Ni { get; set; }
    string? Numsap { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
}

public abstract class AnaliticaBaseModel
{
    public int Id { get; set; }
    public required string Ni { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public partial class ZhrWsAptidaoAptidao : AnaliticaBaseModel, IAnaliticaModel { }
public partial class ZhrWsAtribOrgAtribOrg : AnaliticaBaseModel, IAnaliticaModel { }

