namespace Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

public interface IOutputModel
{
    string Ni { get; set; }
    string Numsap { get; set; }
}

public abstract class ZhrSBaseModelOutput
{
    public DateTimeOffset? UpdatedAt { get; set; }

}
