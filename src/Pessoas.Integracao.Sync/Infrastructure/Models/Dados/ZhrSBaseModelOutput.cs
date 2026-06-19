namespace Pessoas.Integracao.Sync.Infrastructure.Models.Dados;

public interface IOutputModel
{
    string Ni { get; set; }
}

public abstract class ZhrSBaseModelOutput
{
    public DateTime? UpdatedAt { get; set; }

}
