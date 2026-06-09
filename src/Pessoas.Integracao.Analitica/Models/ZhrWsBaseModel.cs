namespace Pessoas.Integracao.Analitica.Models;

public abstract class ZhrWsBaseModel
{
    public int Id { get; set; }
    public required string Ni { get; set; }
    public DateTime? UpdatedAt { get; set; }

}
