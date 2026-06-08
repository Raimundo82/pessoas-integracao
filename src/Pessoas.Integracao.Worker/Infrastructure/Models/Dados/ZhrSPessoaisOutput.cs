namespace Pessoas.Integracao.Worker.Infrastructure.Models.Dados;

public partial class ZhrSPessoaisOutput : ZhrSBaseModelOutput { }

public partial class ZhrSPessoais : ZhrSBaseModel
{
    public int ZhrSPessoaisOutputId { get; set; }
    public virtual required ZhrSPessoaisOutput Root { get; set; }
}

public partial class ZhrSFamilia : ZhrSBaseModel
{
    public int ZhrSPessoaisOutputId { get; set; }
    public virtual required ZhrSPessoaisOutput Root { get; set; }
}

public partial class ZhrSOutrosdados : ZhrSBaseModel
{
    public int ZhrSPessoaisOutputId { get; set; }
    public virtual required ZhrSPessoaisOutput Root { get; set; }
}

public partial class ZhrSDeficiencias : ZhrSBaseModel
{
    public int ZhrSPessoaisOutputId { get; set; }
    public virtual required ZhrSPessoaisOutput Root { get; set; }
}
