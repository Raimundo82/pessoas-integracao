namespace Pessoas.Integracao.Sync.Infrastructure.Models.Dados;

public partial class ZhrSAptidaoOutput : ZhrSBaseModelOutput { }

public partial class ZhrSAptidao : ZhrSBaseModel
{
    public virtual required ZhrSAptidaoOutput Output { get; set; }
}

