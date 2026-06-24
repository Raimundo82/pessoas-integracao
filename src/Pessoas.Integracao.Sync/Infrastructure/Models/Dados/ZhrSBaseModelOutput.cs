namespace Pessoas.Integracao.Sync.Infrastructure.Models.Dados;

public interface IOutputModel
{
    string Ni { get; set; }
    string Numsap { get; set; }
}

public abstract class ZhrSBaseModelOutput
{
    public DateTimeOffset? UpdatedAt { get; set; }

}
