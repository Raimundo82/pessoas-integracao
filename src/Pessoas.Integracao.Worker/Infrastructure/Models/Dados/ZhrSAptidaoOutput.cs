namespace Pessoas.Integracao.Worker.Infrastructure.Models.Dados;

public partial class ZhrSAptidaoOutput : ZhrSBaseModelOutput { }

public partial class ZhrSAptidao : ZhrSBaseModel
{
    public int ZhrSAptidaoOutputId { get; set; }
    public virtual required ZhrSAptidaoOutput Root { get; set; }
}

